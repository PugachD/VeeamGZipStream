using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using VeeamGZipStream.Settings.Mode;

namespace VeeamGZipStream.Tests
{
    [TestClass()]
    public class ValidationParamsTests
    {
        private ValidationParams validation;
        private string sourceFile;
        private string outputFile;

        [TestInitialize]
        public void Init()
        {
            validation = new ValidationParams();
            sourceFile = Path.GetTempFileName();
            outputFile = Path.ChangeExtension(sourceFile, "gz");
        }

        [TestCleanup]
        public void Clean()
        {
            if (File.Exists(sourceFile))
                File.Delete(sourceFile);
        }

        [TestMethod]
        public void NotInputParamsTest()
        {
            //arrange
            string[] argsEmpty = new string[0];
            string message = "Число параметров не соответствует ТЗ";

            //act

            //assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { validation.Read(argsEmpty); }, message);
        }

        [TestMethod]
        public void NullInputParamsTest()
        {
            //arrange
            string[] argsNull = null;
            string message = "Число параметров не соответствует ТЗ";

            //act

            //assert
            Assert.ThrowsException<NullReferenceException>(() => { validation.Read(argsNull); }, message);

        }

        [TestMethod]
        public void ReadValidCompressionModeTest()
        {
            //arrange
            string mode1 = "compress";
            string mode2 = "decompress";
            string mode3 = "cOmpress";
            string mode4 = "dEcompress";

            //act
            var settings1 = validation.Read(new string[] { mode1, sourceFile, outputFile });
            var settings2 = validation.Read(new string[] { mode2, sourceFile, outputFile });
            var settings3 = validation.Read(new string[] { mode3, sourceFile, outputFile });
            var settings4 = validation.Read(new string[] { mode4, sourceFile, outputFile });
            //assert
            Assert.AreEqual(settings1.Mode, Mode.COMPRESS);
            Assert.AreEqual(settings2.Mode, Mode.DECOMPRESS);
            Assert.AreEqual(settings3.Mode, Mode.COMPRESS);
            Assert.AreEqual(settings4.Mode, Mode.DECOMPRESS);
        }

        [TestMethod]
        public void ReadInvalidCompressionModeTest()
        {
            //arrange
            string mode1 = "copres";
            string mode2 = "decopress";

            //act

            //assert
            Assert.ThrowsException<InvalidOperationException>(() => { validation.Read(new string[] { mode1, sourceFile, outputFile }); });
            Assert.ThrowsException<InvalidOperationException>(() => { validation.Read(new string[] { mode2, sourceFile, outputFile }); });
        }

        [TestMethod]
        public void ReadSourceFilePathTest()
        {
            Assert.ThrowsException<FileNotFoundException>(() => validation.Read(new string[] { "compress", " ", outputFile }));
            Assert.ThrowsException<FileNotFoundException>(() => validation.Read(new string[] { "compress", sourceFile + "0", outputFile }));
            Assert.IsNotNull(validation.Read(new string[] { "compress", sourceFile+ " ", outputFile }));
        }

        [TestMethod]
        public void ReadOutputFilePathTest()
        {
            //arrange
            string dir = Path.GetDirectoryName(sourceFile);
            string invalidDirectoryPath = sourceFile.Replace(dir, dir.Remove(dir.Length - 1));
            
            //assert

            Assert.ThrowsException<FileNotFoundException>(() => validation.Read(new string[] { "compress", sourceFile, "" }));
            Assert.ThrowsException<FileNotFoundException>(() => validation.Read(new string[] { "compress", sourceFile, null }));
            Assert.ThrowsException<DirectoryNotFoundException>(() => validation.Read(new string[] { "compress", sourceFile, invalidDirectoryPath }));
            Assert.ThrowsException<FormatException>(() => validation.Read(new string[] { "compress", sourceFile, Path.ChangeExtension(sourceFile,"") }));
            Assert.IsNotNull(validation.Read(new string[] { "compress", sourceFile, outputFile }));

        }
    }
}