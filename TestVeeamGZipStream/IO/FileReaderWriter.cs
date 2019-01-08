using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using TestVeeamGZipStream.Models;

namespace TestVeeamGZipStream.IO
{
    public class FileReaderWriter
    {
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

        public void WriteBlock(Block block)
        {
            try
            {
                while (NumberBlockWriter != block.Number)
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

        public static int NumberBlockWriter { get { return numberBlockWriter; } }

        public List<int> SizeCompressedBlockList { get { return sizeCompressedBlockList; } }

        
    }
}
