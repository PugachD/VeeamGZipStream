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

        private readonly Mode.Mode mode;
        private readonly int blockSize;
        //private readonly int procCount;
        private readonly string sourceFile;
        private readonly string recoverFileName;

        #endregion Fields

        #region .ctor

        public CompressionParams(Mode.Mode mode, string sourceFile, string recoverFileName, int blockSize)
        {
            this.mode = mode;
            this.recoverFileName = recoverFileName;
            this.sourceFile = sourceFile;
            this.blockSize = blockSize;
            //procCount = 1;// Environment.ProcessorCount;
        }

        #endregion .ctor

        #region Properties

        public Mode.Mode Mode
        {
            get { return mode; }
        }

        public int BlockSize
        {
            get { return blockSize; }
        }

        public string SourceFile
        {
            get { return sourceFile; }
        }

        public string RecoverFileName
        {
            get { return recoverFileName; }
        }

        //public int ProcCount
        //{
        //    get { return procCount; }
        //}

        #endregion
    }
}
