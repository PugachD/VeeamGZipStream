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
