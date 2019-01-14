using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestVeeamGZipStream.Settings.Mode;

namespace TestVeeamGZipStream.Settings
{
    public class CompressionParams
    {
        #region Fieds
        
        private const int blockSize = 4 * 1024 * 1024; 

        private readonly Mode.Mode mode;
        private readonly string sourceFile;
        private readonly string recoverFileName;

        #endregion Fields

        #region .ctor

        public CompressionParams(Mode.Mode mode, string sourceFile, string recoverFileName)
        {
            this.mode = mode;
            this.recoverFileName = recoverFileName;
            this.sourceFile = sourceFile;
        }

        #endregion .ctor

        #region Properties

        /// <summary>
        /// Способ сжатия
        /// </summary>
        public Mode.Mode Mode
        {
            get { return mode; }
        }

        /// <summary>
        /// Размер блока для FileStream (потока чтения и записи)
        /// </summary>
        public int BlockSize
        {
            get { return blockSize; }
        }

        /// <summary>
        /// Полное имя исходного файла
        /// </summary>
        public string SourceFile
        {
            get { return sourceFile; }
        }

        /// <summary>
        /// Полное имя выходного файла
        /// </summary>
        public string RecoverFileName
        {
            get { return recoverFileName; }
        }

        #endregion
    }
}
