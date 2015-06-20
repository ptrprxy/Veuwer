using System.IO;

namespace Veuwer.Utility
{
    public class ImageManager
    {
        public static MemoryStream ProcessImage(Stream inStream)
        {
            var outStream = new MemoryStream((int)inStream.Length);

            byte[] header = new byte[2];
            header[0] = (byte)inStream.ReadByte();
            header[1] = (byte)inStream.ReadByte();
            if (header[0] == 0xff && header[1] == 0xd8) //check if it's a jpeg file
            {
                header[0] = (byte)inStream.ReadByte();
                header[1] = (byte)inStream.ReadByte();

                while (header[0] == 0xff && (header[1] >= 0xe0 && header[1] <= 0xef))
                {
                    int exifLength = inStream.ReadByte();
                    exifLength = exifLength << 8;
                    exifLength |= inStream.ReadByte();

                    for (int i = 0; i < exifLength - 2; i++)
                    {
                        inStream.ReadByte();
                    }
                    header[0] = (byte)inStream.ReadByte();
                    header[1] = (byte)inStream.ReadByte();
                }

                outStream.WriteByte(0xff);
                outStream.WriteByte(0xd8);
            }

            outStream.WriteByte(header[0]);
            outStream.WriteByte(header[1]);

            int readCount;
            byte[] readBuffer = new byte[4096];
            while ((readCount = inStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
                outStream.Write(readBuffer, 0, readCount);

            outStream.Flush();
            outStream.Position = 0;
            return outStream;
        }
    }
}