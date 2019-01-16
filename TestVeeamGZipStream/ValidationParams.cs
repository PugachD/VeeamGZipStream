using System;
using System.IO;
using VeeamGZipStream.Settings;
using VeeamGZipStream.Settings.Mode;

namespace VeeamGZipStream
{
    public class ValidationParams
    {
        public CompressionParams Read(string[] args)
        {
            if (args == null)
            {
                throw new NullReferenceException("Число параметров не соответствует ТЗ");
            }
            if (args.Length != 3)
            {
                throw new ArgumentOutOfRangeException("Число параметров не соответствует ТЗ");
            }
            Mode mode = GetMode(args[0]);
            string sourceFile = GetSourceFilePath(args[1]);
            string recoverFileName = GetOutputFilePath(args[2]);

            return new CompressionParams(mode, sourceFile, recoverFileName);
        }

        private Mode GetMode(string mode)
        {
            return Mode.GetByName(mode.ToLower());
        }

        private string GetSourceFilePath(string sourceFilePath)
        {
            sourceFilePath = (sourceFilePath ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(sourceFilePath))
                throw new FileNotFoundException("Путь исходного файла не задан");
            
            if (!File.Exists(sourceFilePath))
                throw new FileNotFoundException("Не найден исходный файл " + sourceFilePath);

            return sourceFilePath;
        }

        private string GetOutputFilePath(string recoverFileName)
        {
            recoverFileName = (recoverFileName ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(recoverFileName))
                throw new FileNotFoundException("Путь результирующего файла не задан");
            
            var dir = Path.GetDirectoryName(recoverFileName);
            if (!Directory.Exists(dir))
                throw new DirectoryNotFoundException("Директория результирующего файла не найдена " + dir);

            bool extension = Path.HasExtension(recoverFileName);
            if (!extension)
            {
                throw new FormatException("Расширение результирующего файла не найдено " + extension);
            }
            // Проверка на наличие самого файла не нужна. 
            // Файл создается или перезаписывается во время открытия пакета FileStream

            return recoverFileName;
        }
    }
}
