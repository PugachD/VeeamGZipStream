using System.IO;
using System.IO.Compression;

namespace TestVeeamGZipStream.IO
{
    public sealed class ReaderWriterFactory
    {
        public FileReaderWriter GetFileReaderWriter(FileStream input, FileStream output)
        {
            return new FileReaderWriter(input, output);
        }
    }
}
