using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestVeeamGZipStream.Models
{
    public class Block
    {
        #region Fields

        private int number;
        private byte[] data;

        #endregion Fields

        public Block(int number, byte[] data)
        {
            this.number = number;
            this.data = data;
        }

        #region Properties

        public int Number
        {
            get { return number; }
            set { number = value; }
        }

        public byte[] Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
            }
        }

        #endregion Properties
    }
}
