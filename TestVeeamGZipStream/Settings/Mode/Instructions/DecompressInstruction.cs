using System.IO;
using System.IO.Compression;
using VeeamGZipStream.Concurrency;
using VeeamGZipStream.IO;
using VeeamGZipStream.Models;

namespace VeeamGZipStream.Settings.Mode.Instructions
{
    public class DecompressInstruction : IGZipInstruction
    {
        public void Processing(UserThreadPool pool, FileReaderWriter readerWriter)
        {
            readerWriter.ReadInfoBlocksAtTheEndFile();
            ///Составление задач
            foreach (var block in readerWriter.SizeCompressedBlockList)
            {
                BlockMetadata metadata = new BlockMetadata(block);
                Task task = new Task(metadata,
                                    (blockData) => Decompress(blockData),
                                    readerWriter);
                pool.Execute(task);
            }
        }
        
        public void PostProcessing(FileReaderWriter readerWriter) { }

        /// <summary>
        /// Применение инструкции декомпрессии
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        private Block Decompress(Block block)
        {
            // Размер жестко фиксируется потому', что декомпрессия производится из меньшего размера в больший 
            // тот, который был до компрессии (CompressInstruction.BLOCK_SIZE)

            using (MemoryStream compressedFileStream = new MemoryStream(CompressInstruction.BLOCK_SIZE))
            {
                using (MemoryStream fileToDecompress = new MemoryStream(block.Data, 0, block.Data.Length))
                {
                    using (GZipStream decompressionStream = new GZipStream(fileToDecompress, CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[CompressInstruction.BLOCK_SIZE];
                        int bytesRead;
                        bytesRead = decompressionStream.Read(buffer, 0, buffer.Length);
                        compressedFileStream.Write(buffer, 0, bytesRead);

                    }
                    block.Data = compressedFileStream.ToArray();
                }
            }
            return block;
        }
    }
}
