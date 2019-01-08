using System.IO;
using System.IO.Compression;
using TestVeeamGZipStream.Models;

namespace TestVeeamGZipStream.Settings.Mode.Instructions
{
    public interface IGZipInstruction
    {
        Block Apply(Block block);

    }
}
