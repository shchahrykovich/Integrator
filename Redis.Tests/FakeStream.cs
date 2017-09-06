using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Redis.Tests
{
    public class FakeStream : Stream
    {
        private readonly byte[] _buffer;
        private int _bufferOffset;

        public FakeStream(String buffer)
        {
            _buffer = Encoding.UTF8.GetBytes(buffer);
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            Array.Copy(_buffer, _bufferOffset, buffer, offset, count);
            _bufferOffset += count;
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => true;
        public override bool CanSeek { get; }
        public override bool CanWrite { get; }
        public override long Length { get; }
        public override long Position { get; set; }
    }
}
