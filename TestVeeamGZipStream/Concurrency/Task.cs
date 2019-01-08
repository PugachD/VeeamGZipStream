using System;
using System.Threading;
using TestVeeamGZipStream.IO;
using TestVeeamGZipStream.Models;
using TestVeeamGZipStream.Settings.Mode.Instructions;

namespace TestVeeamGZipStream.Concurrency
{
    public class Task
    {
        private BlockMetadata metadata;
        private IGZipInstruction instruction;
        private FileReaderWriter readerWriter;

        public Task(BlockMetadata metadata, IGZipInstruction instruction, FileReaderWriter readerWriter)
        {
            this.metadata = metadata;
            this.instruction = instruction;
            this.readerWriter = readerWriter;
        }

        public void Run()
        {
            try
            {
                Block readBlock = readerWriter.ReadBlock(metadata);
                Block updatedBlock = instruction.Apply(readBlock);
                readerWriter.WriteBlock(updatedBlock);
            }
            catch (ThreadInterruptedException ex)
            {
                throw new SystemException(ex.Message);
            }
        }

    }
}
