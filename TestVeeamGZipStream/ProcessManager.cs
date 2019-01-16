using System;
using System.IO;
using System.Threading;
using VeeamGZipStream.Concurrency;
using VeeamGZipStream.IO;
using VeeamGZipStream.Settings;

namespace VeeamGZipStream
{
    public class ProcessManager
    {
        #region Fields

        // Выбираем число потоков равное числу процессоров
        // Выбрал я долгим и нудным тестированием на больших и не очень файлах (до 12 Гигабайт) 
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

        public UserThreadPool Pool
        {
            get { return pool; }
        }

        public void RunProcessManager(CompressionParams settings)
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
                    var readerWriter = readerWriterFactory.GetFileReaderWriter(reader, writer);
                    pool.StartThreadPool();
                    settings.Mode.Instruction.Processing(pool, readerWriter);
                    
                    long res = 0, old = 0;
                    while (!pool.IsFinished())
                    {
                        Thread.Sleep(100);
                        if (!pool.QueueExceptionIsEmpty())
                        {
                            throw pool.GetThreadsException();
                        }
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
