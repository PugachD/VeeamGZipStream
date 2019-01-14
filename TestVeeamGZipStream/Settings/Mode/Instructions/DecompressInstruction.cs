using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using TestVeeamGZipStream.Concurrency;
using TestVeeamGZipStream.IO;
using TestVeeamGZipStream.Models;

namespace TestVeeamGZipStream.Settings.Mode.Instructions
{
    public class DecompressInstruction : IGZipInstruction
    {
        public Block Apply(Block block)
        {
            // Размер жестко фиксируется потому', что декомпрессия производится из меньшего размера в больший 
            // тот, который был до компрессии

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


        public void Processing(UserThreadPool pool, FileReaderWriter readerWriter)
        {
            readerWriter.ReadInfoBlocksAtTheEndFile();
            ///Составление задач
            foreach (var block in readerWriter.SizeCompressedBlockList)
            {
                BlockMetadata metadata = new BlockMetadata(block);
                Task task = new Task(metadata,
                                    (blockData) => Apply(blockData),
                                    readerWriter);
                pool.Execute(task);
            }
        }


        public void PostProcessing(FileReaderWriter readerWriter)
        {
            // Здесь может быть дописана реализация после окончания декомпрессии
            
            // Но это не точно..
        }
    }
}
