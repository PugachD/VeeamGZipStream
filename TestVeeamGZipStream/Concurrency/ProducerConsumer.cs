using System;
using System.Collections.Generic;
using System.Threading;

namespace TestVeeamGZipStream.Concurrency
{
    public class ProducerConsumer<T> where T : class
    {
        object locker = new object(); //Блокируемый объект для синхронизации потоков
        Queue<T> queue = new Queue<T>(); //Очередь задач
        bool isStopped = false;

        public bool IsEmpty
        {
            get { return queue.Count == 0; }
        }

        /// <summary>
        /// Добавление задачи в очередь 
        /// </summary>
        /// <param name="task"></param>
        public void Enqueue(T task)
        {
            if (task == null)
                throw new ArgumentNullException("task");
            lock (locker)
            {
                if (isStopped)
                {
                    throw new InvalidOperationException("Очередь уже остановлена");
                }
                queue.Enqueue(task);
                Monitor.Pulse(locker);
            }
        }

        /// <summary>
        /// Извлечение задачи из очереди 
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            lock (locker)
            {
                while (queue.Count == 0 && !isStopped)
                {
                    Monitor.Wait(locker);
                }
                if (queue.Count == 0)
                {
                    return null;
                }
                return queue.Dequeue();
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
