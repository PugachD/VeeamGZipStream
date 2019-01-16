
namespace VeeamGZipStream.Models
{
    public class BlockMetadata
    {

        private int number;
        private int size;
        private static int counterNumber = 0;


        public BlockMetadata(int size,int number = 0)
        {
            this.number = counterNumber++;
            this.size = size;
        }


        #region Properties

        public int Number
        {
            get { return number; }
            set { number = value; }
        }

        public int Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
            }
        }

        #endregion Properties

    }
}
