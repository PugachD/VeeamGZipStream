using System.IO;
using System.IO.Compression;
using TestVeeamGZipStream.Concurrency;
using TestVeeamGZipStream.IO;
using TestVeeamGZipStream.Models;

namespace TestVeeamGZipStream.Settings.Mode.Instructions
{
    public interface IGZipInstruction
    {
        /// <summary>
        /// Применение инструкции (де)компрессии
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        Block Apply(Block block);

        /// <summary>
        /// Процесс компрессии/декомпрессии.
        /// В него входит чтение предварительных данных для записи 
        /// (информация о блоках, если файла был уже нами сжат)
        /// </summary>
        /// <param name="pool">Пул потоков</param>
        /// <param name="fileReaderWriter">Объект записи-чтения файлов</param>
        void Processing(UserThreadPool pool, FileReaderWriter fileReaderWriter);

        /// <summary>
        /// Остальные операции после компрессии/декомпрессии
        /// </summary>
        /// <param name="readerWriter"></param>
        void PostProcessing(FileReaderWriter readerWriter);
    }
}
