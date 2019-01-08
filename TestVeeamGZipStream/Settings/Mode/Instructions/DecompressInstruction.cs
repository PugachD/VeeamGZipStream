using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using TestVeeamGZipStream.Models;

namespace TestVeeamGZipStream.Settings.Mode.Instructions
{
    public class DecompressInstruction : IGZipInstruction
    {
        public Block Apply(Block block)
        {
            ////!!!!!!!!!!!!!!!!!!!!!!! Размер жестко фиксируется, потому что декомпрессия производится из меньшего размера в больший 
            //////тот, который был до компрессии
            using (MemoryStream compressedFileStream = new MemoryStream(1024*1024))
            {
                using (MemoryStream fileToDecompress = new MemoryStream(block.Data, 0, block.Data.Length))
                {
                    using (GZipStream decompressionStream = new GZipStream(fileToDecompress, CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[1024 * 1024];
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
