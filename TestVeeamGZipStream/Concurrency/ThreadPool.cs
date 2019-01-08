using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TestVeeamGZipStream.Concurrency
{
    public class UserThreadPool
    {
        private readonly AutoResetEvent queueUpdate = new AutoResetEvent(false);
        /// <summary>
        /// Событие синхронизации основного потока
        /// </summary>
        private readonly ManualResetEvent isEnd = new ManualResetEvent(false);
        private object queueLocker = new object();

        private static long tasksInProgress = 0;

        private int nThreads;
        private Thread[] threads;
        private ProducerConsumer<Concurrency.Task> queue;

        public UserThreadPool(int nThreads)
        {
            this.nThreads = nThreads;
            queue = new ProducerConsumer<Concurrency.Task>();
            threads = new Thread[nThreads];

            for (int i = 0; i < nThreads; i++)
            {
                threads[i] = new Thread(Run);
                threads[i].Start();
            }
        }

        public void Execute(Concurrency.Task task)
        {
            queue.Enqueue(task);
            queueUpdate.Set();
        }
        
        /// <summary>
        /// Метод-"колесо" для просмотра задач в очереди
        /// </summary>
        private void Run()
        {
            Concurrency.Task task;

            while (true)
            {
                lock(queueUpdate) {
                    while (queue.IsEmpty)
                    {
                        try
                        {
                            queueUpdate.WaitOne();
                        }
                        catch (ThreadInterruptedException e)
                        {
                            Console.WriteLine("Произошла ошибка во время ожидания очереди: " + e.Message);
                        }
                    }
                    Interlocked.Increment(ref tasksInProgress);
                    task = queue.Dequeue();
                }

                try
                {
                    task.Run();
                    Interlocked.Decrement(ref tasksInProgress);
                }
                catch (SystemException e)
                {
                    Console.WriteLine("Пул потоков прервал работу из-за: " + e.Message);
                }
            }
        }

        public bool IsFinished()
        {
            return queue.IsEmpty && tasksInProgress.Equals(0);
        }

        public void Stop()
        {
            foreach (var thread in threads)
            {
                thread.Abort();
            }
        }
    }
}
