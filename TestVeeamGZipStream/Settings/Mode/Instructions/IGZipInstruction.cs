using VeeamGZipStream.Concurrency;
using VeeamGZipStream.IO;
using VeeamGZipStream.Models;

namespace VeeamGZipStream.Settings.Mode.Instructions
{
    public interface IGZipInstruction
    {
        /// <summary>
        /// Процесс компрессии/декомпрессии.
        /// В него входит чтение предварительных данных для записи 
        /// (информация о блоках, если файл был уже нами сжат)
        /// </summary>
        /// <param name="pool">Пул потоков</param>
        /// <param name="fileReaderWriter">Объект чтения-записи файлов</param>
        void Processing(UserThreadPool pool, FileReaderWriter fileReaderWriter);

        /// <summary>
        /// Остальные операции после компрессии/декомпрессии
        /// </summary>
        /// <param name="readerWriter"></param>
        void PostProcessing(FileReaderWriter readerWriter);
    }
}
