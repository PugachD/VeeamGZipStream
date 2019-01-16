using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VeeamGZipStream.Models;

namespace VeeamGZipStream.Concurrency.Tests
{
    [TestClass()]
    public class ProducerConsumerTests
    {
        [TestMethod()]
        public void EnqueueTest_AddNull()
        {
            //arrange
            var producer = new ProducerConsumer<Block>();

            //act

            //assert
            Assert.ThrowsException<ArgumentNullException>(() => producer.Enqueue(null));
        }

        [TestMethod()]
        public void EnqueueTest_WhenQueueStopped()
        {
            //arrange
            var producer = new ProducerConsumer<Block>();
            var block = new Block(0,null);

            //act
            producer.Stop();

            //assert
            Assert.ThrowsException<InvalidOperationException>(() => producer.Enqueue(block));
        }

        [TestMethod()]
        public void EnqueueTest_SuccessfullyAdding()
        {
            //arrange
            var producer = new ProducerConsumer<Block>();
            var block = new Block(0, null);

            //act
            producer.Enqueue(block);
            producer.Enqueue(block);
            var resultIsEmpty = producer.IsEmpty;

            //assert
            Assert.AreEqual(resultIsEmpty, false);
        }

        [TestMethod()]
        public void DequeueTest_DequeueItemFromStoppedQueueIsNull()
        {
            //arrange
            var producer = new ProducerConsumer<Block>();

            //act
            producer.Stop();
            var resultItemFromDequeue =  producer.Dequeue();

            //assert
            Assert.IsNull(resultItemFromDequeue);
        }

        [TestMethod()]
        public void DequeueTest_DequeueItemFromNotEmptyQueue()
        {
            //arrange
            var producer = new ProducerConsumer<Block>();
            var block = new Block(0, null);

            //act
            producer.Enqueue(block);
            var resultItemFromDequeue = producer.Dequeue();

            //assert
            Assert.IsNotNull(resultItemFromDequeue);
        }

        [TestMethod()]
        public void DoubleStopTest()
        {
            //arrange
            var producer = new ProducerConsumer<Block>();
            var block = new Block(0, null);

            //act
            producer.Stop();
            producer.Stop();

            //assert
            Assert.ThrowsException<InvalidOperationException>(() => producer.Enqueue(block));
        }
    }
}