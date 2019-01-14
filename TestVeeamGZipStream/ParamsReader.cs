using System;
using System.IO;
using TestVeeamGZipStream.Settings;
using TestVeeamGZipStream.Settings.Mode;

namespace TestVeeamGZipStream
{
    public class ParamsReader
    {
        public CompressionParams Read(string[] args)
        {
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
                throw new FileNotFoundException("Путь результирующего фаила не задан");
            
            var dir = Path.GetDirectoryName(recoverFileName);
            if (!Directory.Exists(dir))
                throw new Exception("Директория результирующего файла не найдена " + dir);

            bool extension = Path.HasExtension(recoverFileName);
            if (!extension)
            {
                throw new Exception("Расширение результирующего файла не найдено " + extension);
            }
            // Проверка на наличие самого файла не нужна. 
            // Файл создается или перезаписывается во время открытия пакета FileStream

            return recoverFileName;
        }
    }
}
