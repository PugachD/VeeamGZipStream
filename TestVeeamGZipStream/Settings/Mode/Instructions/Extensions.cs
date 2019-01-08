using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TestVeeamGZipStream.Settings.Mode.Instructions
{
    public static class Extensions
    {
        public static long CopyTo(this Stream source, Stream destination, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            int bytesRead;
            bytesRead = source.Read(buffer, 0, buffer.Length);
                destination.Write(buffer, 0, bytesRead);
            return bytesRead;
        }
    }
}
