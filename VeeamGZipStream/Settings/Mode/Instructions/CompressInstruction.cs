using VeeamGZipStream.Models;
using System.IO.Compression;
using System.IO;
using VeeamGZipStream.Concurrency;
using VeeamGZipStream.IO;

namespace VeeamGZipStream.Settings.Mode.Instructions
{
    public class CompressInstruction : IGZipInstruction
    {
        /// <summary>
        /// Размер сжимаемых блоков: 1 Мбайт
        /// </summary>
        public const int BLOCK_SIZE = 1024 * 1024;

        public void Processing(UserThreadPool pool, FileReaderWriter readerWriter)
        {
            int blockCount = readerWriter.GetNumberOfBlocks(BLOCK_SIZE);
            for (int i = 0; i < blockCount; i++)
            {
                BlockMetadata metadata = new BlockMetadata(BLOCK_SIZE);
                Task task = new Task(metadata, 
                                    (blockData) => Compress(blockData),
                                    readerWriter);
                pool.Execute(task);
            }
        }


        public void PostProcessing(FileReaderWriter readerWriter)
        {
            readerWriter.WriteInfoBlocksAtTheEndFile();
        }


        private Block Compress(Block block)
        {
            using (MemoryStream outStream = new MemoryStream(block.Data.Length))
            {
                using (GZipStream compressionStream = new GZipStream(outStream, CompressionMode.Compress))
                {
                    compressionStream.Write(block.Data, 0, block.Data.Length);
                }
                block.Data = outStream.ToArray();

            }
            return block;
        }
    }
}
