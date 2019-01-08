using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using TestVeeamGZipStream.Concurrency;
using TestVeeamGZipStream.IO;
using TestVeeamGZipStream.Models;
using TestVeeamGZipStream.Settings;
using TestVeeamGZipStream.Settings.Mode;

namespace TestVeeamGZipStream
{
    public class ProcessManager
    {
        #region Fields

        private const int BLOCK_SIZE = 1024 * 1024;
        //Выбираем (выбрал я долгим и нудным тестированием на больших и не очень файлах) число потоков равное числу процессоров
        private readonly int threadCount = Environment.ProcessorCount;

        private UserThreadPool pool;
        private ReaderWriterFactory readerWriterFactory;

        #endregion Fields

        #region .ctor

        public ProcessManager(ReaderWriterFactory readerWriterFactory)
        {
            this.readerWriterFactory = readerWriterFactory;
            this.pool = new UserThreadPool(threadCount);
        }

        #endregion .ctor

        public void Run(CompressionParams settings)
        {
            using (var reader = new FileStream(settings.SourceFile,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            settings.BlockSize,
            FileOptions.Asynchronous))
            {
                using (var writer = new FileStream(settings.RecoverFileName,
                    FileMode.OpenOrCreate,
                    FileAccess.Write,
                    FileShare.Write,
                    settings.BlockSize,
                    FileOptions.Asynchronous))
                {

                    FileReaderWriter readerWriter = readerWriterFactory.GetFileReaderWriter(reader, writer);
                    int blockCount;
                    ///Чтение с конца файла информации о сжатии
                    if (settings.Mode == Mode.DECOMPRESS)
                    {
                        using (MemoryStream compressInfoStream = new MemoryStream())
                        {
                            int offset = sizeof(int);
                            reader.Position = reader.Length - offset;
                            byte[] countBlocks = new byte[sizeof(int)];
                            reader.Read(countBlocks, 0, sizeof(int));
                            blockCount = BitConverter.ToInt32(countBlocks, 0);
                            //сдвигаем позицию к началу списка размеров блоков
                            offset += sizeof(int) * blockCount;
                            reader.Position = reader.Length - offset;
                            //Проходим каждые 4 байта (размер инта) и считываем значение блока в список
                            for (int i = 0; i < blockCount; i++)
                            {
                                byte[] sizeBlock = new byte[sizeof(int)];
                                reader.Read(sizeBlock, 0, sizeof(int));
                                readerWriter.SizeCompressedBlockList.Add(BitConverter.ToInt32(sizeBlock, 0));
                            }

                            reader.Position = 0;
                        }
                        ///Составление задач
                        foreach (var block in readerWriter.SizeCompressedBlockList)
                        {
                            BlockMetadata metadata = new BlockMetadata(block);
                            Task task = new Task(metadata, settings.Mode.Instruction, readerWriter);
                            pool.Execute(task);
                        }
                    }
                    else
                    {
                        blockCount = readerWriter.GetNumberOfBlocks(BLOCK_SIZE);
                        ///
                        for (int i = 0; i < blockCount; i++)
                        {
                            BlockMetadata metadata = new BlockMetadata(BLOCK_SIZE);
                            Task task = new Task(metadata, settings.Mode.Instruction, readerWriter);
                            pool.Execute(task);
                        }
                    }

                    long res = 0, old = 0;
                    while (!pool.IsFinished())
                    {
                        Thread.Sleep(100);
                        if (old < (res = (100 * reader.Position) / reader.Length))
                                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " Прогресс: " + (old = res) + "%");
                    }
                    ///Запись в конец файла информации о сжатии
                    if (settings.Mode == Mode.COMPRESS)
                    {
                        using (MemoryStream compressInfoStream = new MemoryStream())
                        {
                            foreach (var item in readerWriter.SizeCompressedBlockList)
                            {
                                compressInfoStream.Write(BitConverter.GetBytes(item), 0, sizeof(int));
                            }
                            compressInfoStream.Write(BitConverter.GetBytes(FileReaderWriter.NumberBlockWriter), 0, sizeof(int));
                            writer.Write(compressInfoStream.ToArray(), 0, (int)compressInfoStream.Length);
                        }
                    }
                    ///
                    pool.Stop();
                }
            }
        }
    }
}
