using System;
using System.Collections.Generic;
using System.Threading;

namespace TestVeeamGZipStream.Concurrency
{
    public class ProducerConsumer<T> where T : class
    {
        private object locker = new object();
        private Queue<T> queueTasks = new Queue<T>(); 
        private bool isStopped = false;
        
        public bool IsEmpty
        {
            get { return queueTasks.Count == 0; }
        }

        /// <summary>
        /// Добавление задачи в очередь 
        /// </summary>
        public void Enqueue(T task)
        {
            if (task == null)
                throw new ArgumentNullException("Задача для добавления в очередь пуста");
            lock (locker)
            {
                if (isStopped)
                {
                    throw new InvalidOperationException("Очередь задач уже остановлена. В нее больше нельзя добавлять задачи.");
                }
                queueTasks.Enqueue(task);
                Monitor.Pulse(locker);
            }
        }

        /// <summary>
        /// Извлечение задачи из очереди 
        /// </summary>
        public T Dequeue()
        {
            lock (locker)
            {
                while (queueTasks.Count == 0 && !isStopped)
                {
                    Monitor.Wait(locker);
                }
                if (queueTasks.Count == 0)
                {
                    return null;
                }
                return queueTasks.Dequeue();
            }
        }
        
        public void Stop()
        {
            lock (locker)
            {
                isStopped = true;
                Monitor.PulseAll(locker);
            }
        }
    }
}
