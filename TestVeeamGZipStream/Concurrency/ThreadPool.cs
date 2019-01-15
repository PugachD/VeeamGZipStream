using System;
using System.Threading;

namespace TestVeeamGZipStream.Concurrency
{
    public class UserThreadPool
    {
        private readonly AutoResetEvent queueTasksUpdate = new AutoResetEvent(false);
        private readonly ProducerConsumer<Concurrency.Task> queueTasks;
        private readonly ProducerConsumer<Exception> queueExceptions;

        private static long tasksInProgress = 0;

        private int nThreads;
        private Thread[] threads;

        private bool poolIsStarted = false;

        public UserThreadPool(int nThreads)
        {
            this.nThreads = nThreads;
            queueTasks = new ProducerConsumer<Concurrency.Task>();
            queueExceptions = new ProducerConsumer<Exception>();
            threads = new Thread[nThreads];
        }

        public void Execute(Concurrency.Task task)
        {
            queueTasks.Enqueue(task);
            queueTasksUpdate.Set();
        }
        
        /// <summary>
        /// Запуск пула потоков
        /// </summary>
        public void StartThreadPool()
        {
            if (!poolIsStarted)
            {
                for (int i = 0; i < nThreads; i++)
                {
                    threads[i] = new Thread(RunThread);
                    threads[i].Name = (i+1).ToString();
                    threads[i].Start();
                }
                poolIsStarted = true;
            }
        }

        private void RunThread()
        {
            try
            {
                DistributeTasks();
            }
            catch (ThreadAbortException abortEx)
            {
                // Не обрабатываем принудительное прерывание потока
                if (poolIsStarted) throw abortEx;
            }
            catch (Exception ex)
            {
                queueExceptions.Enqueue(ex);
            }
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
                lock(queueTasksUpdate)
                {
                    while (queueTasks.IsEmpty)
                    {
                        try
                        {
                            queueTasksUpdate.WaitOne();
                        }
                        catch (ThreadInterruptedException e)
                        {
                            throw new ThreadInterruptedException(String.Format("Произошла ошибка во время ожидания очереди для потока {0}: ",
                                                                Thread.CurrentThread.Name) 
                                                                + e.Message);
                        }
                    }
                    Interlocked.Increment(ref tasksInProgress);
                    task = queueTasks.Dequeue();
                }

                try
                {
                    task.StartOperationOnBlock();
                    Interlocked.Decrement(ref tasksInProgress);
                }
                catch (SystemException e)
                {
                    throw new SystemException(String.Format("Поток {0} из пула потоков прервал работу из-за: ",
                                                            Thread.CurrentThread.Name) 
                                              + e.Message);
                }
            }
        }

        public bool QueueExceptionIsEmpty()
        {
            bool ret = false; ;
            lock (queueExceptions)
            {
                ret = queueExceptions.IsEmpty;
            }
            return ret;
        }

        public Exception GetThreadsException()
        {
            return queueExceptions.Dequeue();
        }

        public bool IsFinished()
        {
            return queueTasks.IsEmpty && tasksInProgress.Equals(0);
        }

        public void Stop()
        {
            if (poolIsStarted)
            {
                poolIsStarted = false;
                foreach (var thread in threads)
                {
                    thread.Abort();
                }
            }
        }
    }
}
