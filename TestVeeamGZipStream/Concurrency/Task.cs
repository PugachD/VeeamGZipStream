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

        public void Run()
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
        }

    }
}
