using System;
using TestVeeamGZipStream.Models;
using System.IO.Compression;
using System.IO;

namespace TestVeeamGZipStream.Settings.Mode.Instructions
{
    public class CompressInstruction : IGZipInstruction
    {
        public Block Apply(Block block)
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
