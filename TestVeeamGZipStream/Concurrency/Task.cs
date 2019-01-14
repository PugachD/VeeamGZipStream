using System;
using System.Threading;
using TestVeeamGZipStream.IO;
using TestVeeamGZipStream.Models;

namespace TestVeeamGZipStream.Concurrency
{
    public class Task
    {
        private BlockMetadata metadata;
        private Func<Block, Block> applyInstructions;
        private FileReaderWriter readerWriter;

        public Task(BlockMetadata metadata, Func<Block, Block> applyInstructions, FileReaderWriter readerWriter)
        {
            this.metadata = metadata;
            this.applyInstructions = applyInstructions;
            this.readerWriter = readerWriter;
        }

        /// <summary>
        /// Запускает задачу чтения из потока, операции (де)компрессии и записи в поток
        /// </summary>
        public void StartOperationOnBlock()
        {
            try
            {
                Block readBlock = readerWriter.ReadBlock(metadata);
                Block updatedBlock = applyInstructions(readBlock);
                readerWriter.WriteBlock(updatedBlock);
            }
            catch (ThreadInterruptedException ex)
            {
                throw new SystemException(ex.Message);
            }
            catch(Exception ex)
            {
                throw new SystemException(ex.Message);
            }
        }

    }
}
