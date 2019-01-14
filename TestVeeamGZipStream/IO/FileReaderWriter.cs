using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TestVeeamGZipStream.Models;

namespace TestVeeamGZipStream.IO
{
    public class FileReaderWriter
    {
        private const int SizeOfWriteInfoBlock = 4;

        private readonly FileStream reader;
        private readonly FileStream writer;
        public static object lockReader = new object();
        public static object lockWriter = new object();
        /// <summary>
        /// Номер блока, который должен быть считан (для правильной очередности из компрессированного файла)
        /// </summary>
        private static int numberBlockReader = 0;
        /// <summary>
        /// Номер блока, который должен быть записан (для правильной очередности)
        /// </summary>
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

        public int GetNumberOfBlocks(int blockSize)
        {
            try
            {
                return (int)Math.Ceiling(reader.Length  / (double)blockSize); 
            }
            catch (IOException e)
            {
                throw new IOException("Не удалось прочитать файл");
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
                    // не знаю норм ли подход, но пока так. И даже не так. Долгим тестированием выяснено, что засыпание не ускоряет процесс поиска, 
                    // так как операции чтения-записи небольших блоков работы 4х-8 потоков выполняются достаточно быстро и цикл работает не столь долго
                }
                lock (lockReader)
                {
                    bytesRead = reader.Read(data, 0, metadata.Size);
                    numberBlockReader++;
                    //metadata.Number = numberBlockReader++;
                }
                ///Делается один раз с самым последним блоком. И то при сжатии, потому что он не содержит столько байт сколько мы указываем
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
                throw new IOException("Не удалось прочитать файл");
            }
        }
        
        /// <summary>
        /// Чтение информации о сжатых блоках с конца файла после компрессии.
        /// </summary>
        public void ReadInfoBlocksAtTheEndFile()
        {
            // Данные читаются с конца файла. Последним идет количество блоков. 
            // Далее сдвигаемся с конца на (количество блоков * размер записываемых данных (4 байта))
            // Это нужно для верной последовательности. Считываем размер каждого блока и заносим в массив.
            int blockCount;
            ///Чтение с конца файла информации о сжатии
            using (MemoryStream compressInfoStream = new MemoryStream())
            {
                int offset = SizeOfWriteInfoBlock;
                reader.Position = reader.Length - offset;
                byte[] countBlocks = new byte[SizeOfWriteInfoBlock];
                reader.Read(countBlocks, 0, SizeOfWriteInfoBlock);
                blockCount = BitConverter.ToInt32(countBlocks, 0);
                //сдвигаем позицию к началу списка размеров блоков
                offset += SizeOfWriteInfoBlock * blockCount;
                reader.Position = reader.Length - offset;
                //Проходим каждые 4 байта (размер инта) и считываем значение блока в список
                for (int i = 0; i < blockCount; i++)
                {
                    byte[] sizeBlock = new byte[SizeOfWriteInfoBlock];
                    reader.Read(sizeBlock, 0, SizeOfWriteInfoBlock);
                    sizeCompressedBlockList.Add(BitConverter.ToInt32(sizeBlock, 0));
                }

                reader.Position = 0;
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
                    // не знаю норм ли подход, но пока так. И даже не так. Долгим тестированием выяснено, что засыпание не ускоряет процесс поиска, 
                    // так как операции чтения-записи небольших блоков работы 4х-8 потоков выполняются достаточно быстро и цикл работает не столь долго
                }
                lock (writer)
                {
                    writer.Write(block.Data, 0, block.Data.Length);
                    numberBlockWriter++;
                    sizeCompressedBlockList.Add(block.Data.Length);
                }
            }
            catch (IOException e)
            {
                throw new IOException("Не удалось записать файл");
            }
        }


        /// <summary>
        /// Запись информации о сжатых блоках в конец файла после компрессии.
        /// После последнего сомпрессированного блока записываются размеры всех блоков
        /// в порядке записи, последним в файл заносится число сжатых блоков.
        /// </summary>
        public void WriteInfoBlocksAtTheEndFile()
        {
            using (MemoryStream compressInfoStream = new MemoryStream())
            {
                foreach (var item in SizeCompressedBlockList)
                {
                    compressInfoStream.Write(BitConverter.GetBytes(item), 0, SizeOfWriteInfoBlock);
                }
                compressInfoStream.Write(BitConverter.GetBytes(numberBlockWriter), 0, SizeOfWriteInfoBlock);
                writer.Write(compressInfoStream.ToArray(), 0, (int)compressInfoStream.Length);
            }
        }
        
        #endregion --------------- Методы чтения --------------------------

    }
}
