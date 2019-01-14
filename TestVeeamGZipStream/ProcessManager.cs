using System;
using System.IO;
using System.Threading;
using TestVeeamGZipStream.Concurrency;
using TestVeeamGZipStream.IO;
using TestVeeamGZipStream.Settings;

namespace TestVeeamGZipStream
{
    public class ProcessManager
    {
        #region Fields

        //Выбираем (выбрал я долгим и нудным тестированием на больших и не очень файлах) число потоков равное числу процессоров
        private readonly int threadCount = Environment.ProcessorCount;

        private readonly ReaderWriterFactory readerWriterFactory;
        private readonly UserThreadPool pool;

        #endregion Fields

        #region .ctor

        public ProcessManager()
        {
            readerWriterFactory = new ReaderWriterFactory();
            pool = new UserThreadPool(threadCount);
        }

        #endregion .ctor

        public void Run(CompressionParams settings)
        {
            //Открываем поток чтения файла
            using (var reader = new FileStream(settings.SourceFile,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            settings.BlockSize,
            FileOptions.Asynchronous))
            {
                //Открываем поток записи файла
                using (var writer = new FileStream(settings.RecoverFileName,
                    FileMode.OpenOrCreate,
                    FileAccess.Write,
                    FileShare.Write,
                    settings.BlockSize,
                    FileOptions.Asynchronous))
                {
                    var readerWriter = readerWriterFactory.GetFileReaderWriter(reader, writer);
                    settings.Mode.Instruction.Processing(pool, readerWriter);
                    
                    long res = 0, old = 0;
                    while (!pool.IsFinished())
                    {
                        Thread.Sleep(100);
                        if (old < (res = (100 * reader.Position) / reader.Length))
                                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " Прогресс: " + (old = res) + "%");
                    }

                    settings.Mode.Instruction.PostProcessing(readerWriter);

                    pool.Stop();
                }
            }
        }
    }
}
