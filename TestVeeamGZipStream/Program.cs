using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using TestVeeamGZipStream.IO;
using TestVeeamGZipStream.Settings;
using TestVeeamGZipStream.Settings.Mode;
using TestVeeamGZipStream.Settings.Mode.Instructions;

namespace TestVeeamGZipStream
{
    class Program
    {
        static void Main(string[] args)
        {
            string sourceFile = @"E:\Раздачи\Пхукет\[R.G. Mechanics] Mass Effect Galaxy Edition\02. Mass Effect 2\data1.bin";//@"E:\Source — копия.txt";//  E:\Games\WOT\res\packages\vehicles_level_07-part1.pkg";
            string outputFile = @"E:\Output.txt"; 
            int blockSize = 4 * 1024 * 1024; // размер блока
            ReaderWriterFactory factory = new ReaderWriterFactory();
            ProcessManager manager = new ProcessManager(factory);
            //manager.Run(new CompressionParams(Mode.COMPRESS, sourceFile, outputFile,  blockSize));
            manager.Run(new CompressionParams(Mode.DECOMPRESS, outputFile, "E:\\Source_new_1.txt", blockSize));

            Console.WriteLine("Алгоритм закончил работу. Нажмите любую клавишу для выхода");
            Console.ReadLine();
        }
    }
}
