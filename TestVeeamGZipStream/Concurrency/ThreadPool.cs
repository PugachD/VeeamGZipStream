using System;
using System.Threading;

namespace TestVeeamGZipStream.Concurrency
{
    public class UserThreadPool
    {
        private readonly AutoResetEvent queueUpdate = new AutoResetEvent(false);

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
                threads[i] = new Thread(DistributeTasks);
                threads[i].Start();
            }
        }

        public void Execute(Concurrency.Task task)
        {
            queue.Enqueue(task);
            queueUpdate.Set();
        }
        
        /// <summary>
        /// Метод - "колесо" для просмотра задач в очереди.
        /// И распределения полученных задач потокам.
        /// </summary>
        private void DistributeTasks()
        {
            Concurrency.Task task;

            while (true)
            {
                lock(queueUpdate)
                {
                    while (queue.IsEmpty)
                    {
                        try
                        {
                            queueUpdate.WaitOne();
                        }
                        catch (ThreadInterruptedException e)
                        {
                            throw new ThreadInterruptedException("Произошла ошибка во время ожидания очереди: " + e.Message);
                        }
                    }
                    Interlocked.Increment(ref tasksInProgress);
                    task = queue.Dequeue();
                }

                try
                {
                    task.StartOperationOnBlock();
                    Interlocked.Decrement(ref tasksInProgress);
                }
                catch (SystemException e)
                {
                    throw new SystemException("Пул потоков прервал работу из-за: " + e.Message);
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
