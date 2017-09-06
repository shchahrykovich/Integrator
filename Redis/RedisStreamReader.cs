using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Redis
{
    public class RedisStreamReader
    {
        private readonly byte[] _temp;
        private int _currentLength = 0;
        private int _currentPosition = 0;
        private readonly Stream _inner;
        private readonly CancellationToken _token;

        public RedisStreamReader(Stream inner, CancellationToken token, int bufferLength)
        {
            _inner = inner;
            _token = token;
            _temp = new byte[bufferLength];
        }

        private bool CanRead(int length)
        {
            return _currentPosition + length < _currentLength;
        }

        public byte[] ReadBytes(int length)
        {
            var content = new byte[length];
            var sourceIndex = 0;
            while (sourceIndex != length)
            {
                int currentReadLength = length - sourceIndex;
                if (_currentLength < _currentPosition + currentReadLength)
                {
                    currentReadLength = _currentLength - _currentPosition;
                    Array.Copy(_temp, _currentPosition, content, sourceIndex, currentReadLength);
                    _currentPosition += currentReadLength;
                    ReadFromStream();
                }
                else
                {
                    Array.Copy(_temp, _currentPosition, content, sourceIndex, currentReadLength);
                    _currentPosition += currentReadLength;
                }

                sourceIndex += currentReadLength;
            }

            return content;
        }

        public byte? ReadByte()
        {
            if (!CanRead(1))
            {
                ReadFromStream();
            }

            if (_currentLength == 0)
            {
                return null;
            }
            var result = _temp[_currentPosition];
            _currentPosition++;
            return result;
        }

        private void ReadFromStream()
        {
            if (_currentPosition == _currentLength)
            {
                _currentLength = _inner.ReadAsync(_temp, 0, _temp.Length, _token).Result;
                _currentPosition = 0;
            }
        }

        public void ReadEndLine()
        {
            var carriageReturn = (char) ReadByte();
            var newLine = (char) ReadByte();
            if ('\r' != carriageReturn || '\n' != newLine)
            {
                throw new ApplicationException("Can't read end line");
            }
        }

        public string ReadLine()
        {
            List<byte> content = new List<byte>();
            var carriageReturn = (char) ReadByte();
            content.Add((byte) carriageReturn);

            var newLine = (char) ReadByte();
            content.Add((byte) newLine);

            while (carriageReturn != '\r' && newLine != '\n')
            {
                var current = (char) ReadByte();
                content.Add((byte) current);

                carriageReturn = newLine;
                newLine = current;
            }

            var result = Encoding.UTF8.GetString(content.ToArray());
            return result.Substring(0, result.Length - 2);
        }
    }
}