using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Veuwer.Utility
{
    public class LengthStream : Stream
    {
        public override bool CanRead { get { return stream.CanRead; } }
        public override bool CanSeek { get { return true; } }
        public override bool CanWrite { get { return stream.CanWrite; } }
        public override long Length { get { return length; } }
        public override long Position { get { return stream.Position; } set { stream.Position = value; } }

        Stream stream;
        long length;

        public LengthStream(Stream stream, long length)
        {
            this.stream = stream;
            this.length = length;
        }

        public override void Flush()
        {
            stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            stream.Write(buffer, offset, count);
        }
    }
}