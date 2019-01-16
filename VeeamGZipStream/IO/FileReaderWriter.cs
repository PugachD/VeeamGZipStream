using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VeeamGZipStream.Models;

namespace VeeamGZipStream.IO
{
    public class FileReaderWriter
    {
        private const int SizeOfWriteInfoBlock = 4;
        private const string stringUserID = "Dmitriy";
        private readonly byte[] bytesUserID = Encoding.Unicode.GetBytes(stringUserID);

        private readonly FileStream reader;
        private readonly FileStream writer;
        public static object lockReader = new object();
        public static object lockWriter = new object();
        private static int numberBlockReader = 0;
        private static int numberBlockWriter = 0;

        private readonly List<int> sizeCompressedBlockList = new List<int>();

        public FileReaderWriter(FileStream input, FileStream output)
        {
            this.reader = input;
            this.writer = output;
        }

        /// <summary>
        /// Список размеров сжатых блоков
        /// </summary>
        public IEnumerable<int> SizeCompressedBlockList { get { return sizeCompressedBlockList; } }

        /// <summary>
        /// Получить число блоков в файле.
        /// </summary>
        /// <param name="blockSize">Размер блока, по которому надо узнать количество помещающихся в файл блоков</param>
        public int GetNumberOfBlocks(int blockSize)
        {
            try
            {
                return (int)Math.Ceiling(reader.Length  / (double)blockSize); 
            }
            catch (IOException e)
            {
                throw new IOException("Не удалось прочитать файл:\n" + e.Message);
            }
        }

        #region --------------- Методы записи --------------------------

        public Block ReadBlock(BlockMetadata metadata)
        {
            try
            {
                byte[] data = new byte[metadata.Size];
                int bytesRead;
                while (numberBlockReader != metadata.Number)
                {
                    // Начинаем читать только в упорядоченном направлении.
                    // Долгим тестированием выяснено, что засыпание (Thread.Sleep() не ускоряет процесс поиска, 
                    // так как операции чтения-записи на установленных нами размерах (1 МБайт) блоков 
                    // работы 2х-8 потоков выполняются достаточно быстро и цикл работает не столь долго
                }
                lock (lockReader)
                {
                    bytesRead = reader.Read(data, 0, metadata.Size);
                    numberBlockReader++;
                }

                /// Делается один раз с самым последним блоком. И то при сжатии, потому что он может не содержать столько байт сколько мы указываем
                if (bytesRead < metadata.Size)
                {
                    metadata.Size = bytesRead;
                    byte[] shortArray = data.Take(bytesRead).ToArray();
                    data = null;
                    return new Block(metadata.Number, shortArray);
                }
                ///
                return new Block(metadata.Number, data);
            }
            catch (IOException e)
            {
                throw new IOException("Не удалось прочитать блок данных с номером {0} из файла", metadata.Number);
            }
        }

        /// <summary>
        /// Чтение информации о сжатых блоках с конца файла, записанных после компрессии.
        /// </summary>
        /// <!--
        /// Дополнительно: 
        /// Данные читаются с конца файла. Последним идет количество сжатых блоков. 
        /// Далее сдвигаемся с конца на (количество блоков * размер записываемых данных (4 байта))
        /// Это нужно для верной последовательности. Считываем размер каждого блока и заносим в массив.
        /// -->
        public void ReadInfoBlocksAtTheEndFile()
        {
            int blockCount;
            ///Чтение с конца файла информации о сжатии
            using (MemoryStream compressInfoStream = new MemoryStream())
            {
                /// Проверка на маркер в файле
                int offset = bytesUserID.Length;
                reader.Position = reader.Length - offset;
                byte[] userIDInFile = new byte[bytesUserID.Length];
                reader.Read(userIDInFile, 0, bytesUserID.Length);
                СompressionFileWasCompressed(userIDInFile);
                /// Окончена проверка на маркер в файле
                
                offset += SizeOfWriteInfoBlock;
                reader.Position = reader.Length - offset;
                byte[] countBlocks = new byte[SizeOfWriteInfoBlock];
                reader.Read(countBlocks, 0, SizeOfWriteInfoBlock);
                blockCount = BitConverter.ToInt32(countBlocks, 0);
                // Сдвигаем позицию к началу списка размеров блоков
                offset += SizeOfWriteInfoBlock * blockCount;
                reader.Position = reader.Length - offset;
                // Проходим каждые 4 байта (размер инта) и считываем значение блока в список
                for (int i = 0; i < blockCount; i++)
                {
                    byte[] sizeBlock = new byte[SizeOfWriteInfoBlock];
                    reader.Read(sizeBlock, 0, SizeOfWriteInfoBlock);
                    sizeCompressedBlockList.Add(BitConverter.ToInt32(sizeBlock, 0));
                }

                reader.Position = 0;
            }
        }

        private void СompressionFileWasCompressed(byte[] bytes)
        {
            string readString = Encoding.Unicode.GetString(bytes);
            if (readString != stringUserID)
            {
                throw new InvalidDataException(String.Format("Выбрнайл для декомпрессии файл был сформирован не нами." +
                                            "\nНе найдено ключевое слово {0} в потайном месте :).", stringUserID));
            }
        }

        #endregion --------------- Методы записи --------------------------

        #region --------------- Методы чтения --------------------------

        public void WriteBlock(Block block)
        {
            try
            {
                while (numberBlockWriter != block.Number)
                {
                    // Долгим тестированием выяснено, что засыпание (Thread.Sleep() не ускоряет процесс поиска, 
                    // так как операции чтения-записи на установленных нами размерах (1 МБайт) блоков 
                    // работы 2х-8 потоков выполняются достаточно быстро и цикл работает не столь долго
                }
                lock (writer)
                {
                    writer.Write(block.Data, 0, block.Data.Length);
                    numberBlockWriter++;
                    sizeCompressedBlockList.Add(block.Data.Length);
                }
            }
            catch (Exception e)
            {
                throw new IOException("Не удалось записать блок данных с номером {0} в  файл по причине", block.Number);
            }
        }


        /// <summary>
        /// Запись информации о сжатых блоках в конец файла после компрессии.
        /// </summary>
        /// <!--
        /// После последнего сомпрессированного блока записываются размеры всех блоков
        /// в порядке записи, последним в файл заносится число сжатых блоков.
        /// -->
        public void WriteInfoBlocksAtTheEndFile()
        {
            using (MemoryStream compressInfoStream = new MemoryStream())
            {
                foreach (var item in SizeCompressedBlockList)
                {
                    compressInfoStream.Write(BitConverter.GetBytes(item), 0, SizeOfWriteInfoBlock);
                }
                compressInfoStream.Write(BitConverter.GetBytes(numberBlockWriter), 0, SizeOfWriteInfoBlock);
                compressInfoStream.Write(bytesUserID, 0, bytesUserID.Length);
                writer.Write(compressInfoStream.ToArray(), 0, (int)compressInfoStream.Length);
            }
        }
        
        #endregion --------------- Методы чтения --------------------------

    }
}
