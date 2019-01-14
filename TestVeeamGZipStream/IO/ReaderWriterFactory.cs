using System.IO;
using System.IO.Compression;

namespace TestVeeamGZipStream.IO
{
    public sealed class ReaderWriterFactory
    {
        /// <summary>
        /// Получаем объект чтения-записи
        /// </summary>
        /// <param name="reader">Поток чтения</param>
        /// <param name="writer">Поток записи</param>
        /// <returns></returns>
        public FileReaderWriter GetFileReaderWriter(FileStream reader, FileStream writer)
        {
            return new FileReaderWriter(reader, writer);
        }
    }
}
