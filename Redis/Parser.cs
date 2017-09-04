using Redis.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Redis
{
    public class Parser
    {
        public RedisToken Parse(Stream reader, CancellationToken cancellationToken)
        {
            var type = new byte[1];
            var readLen = reader.ReadAsync(type, 0, 1, cancellationToken).Result;
            if (0 == readLen)
            {
                return null;
            }
            switch ((char) type[0])
            {
                case '*':
                {
                    var length = int.Parse(ReadLine(reader, cancellationToken));
                    var token = new ArrayRedisToken(length);
                    for (int i = 0; i < length; i++)
                    {
                        token.Add(Parse(reader, cancellationToken));
                    }
                    return token;
                }
                case '+':
                {
                    return new SimpleStringRedisToken
                    {
                        Data = ReadLine(reader, cancellationToken)
                    };
                }
                case ':':
                {
                    return new IntegerRedisToken
                    {
                        Data = int.Parse(ReadLine(reader, cancellationToken))
                    };
                }
                case '$':
                {
                    var length = int.Parse(ReadLine(reader, cancellationToken));
                    var content = new byte[length];
                    int result = 0;
                    int read = 0;
                    int offset = 0;
                    do
                    {
                        result = reader.Read(content, offset, length);
                        offset = result;
                        length = length - result;
                    } while (read + offset != content.Length);
                    ReadEndLine(reader, cancellationToken);
                    return new BulkStringRedisToken()
                    {
                        Content = content

                    };
                }
                default:
                    throw new NotImplementedException();
            }
        }

        private void ReadEndLine(Stream reader, CancellationToken cancellationToken)
        {
            if ('\r' != ReadByte(reader, cancellationToken) || '\n' != ReadByte(reader, cancellationToken))
            {
                throw  new ApplicationException("Can't read end line");
            }
        }

        private string ReadLine(Stream s, CancellationToken cancellationToken)
        {
            List<byte> content = new List<byte>();

            var carriageReturn = (char)ReadByte(s, cancellationToken);
            content.Add((byte)carriageReturn);

            var newLine = (char)ReadByte(s, cancellationToken);
            content.Add((byte)newLine);

            while (carriageReturn != '\r' && newLine != '\n')
            {
                var current = (char) ReadByte(s, cancellationToken);
                content.Add((byte)current);

                carriageReturn = newLine;
                newLine = current;
            }

            var result = Encoding.UTF8.GetString(content.ToArray());
            return result.Substring(0, result.Length -2);
        }

        private byte ReadByte(Stream reader, CancellationToken token)
        {
            var temp = new byte[1];

            Task<int> task;
            do
            {
                task = reader.ReadAsync(temp, -0, 1, token);
                task.Wait(token);
            } while (task.Result == 0);

            return temp[0];
        }

        public void ConvertToString(RedisToken token, Stream writer)
        {
            if (token is ArrayRedisToken array)
            {
                var len = array.Items.Count();
                WriteString("*" + len, writer);
                WriteNewLine(writer);
                for (int i = 0; i < len; i++)
                {
                    ConvertToString(array.Items.ElementAt(i), writer);
                }
            }
            else if (token is SimpleStringRedisToken simple)
            {
                WriteString("+" + simple.Data, writer);
                WriteNewLine(writer);
            }
            else if (token is BulkStringRedisToken bulk)
            {
                if (null == bulk.Content)
                {
                    WriteString("$-1\r\n", writer);
                }
                else
                {
                    WriteString("$" + bulk.Content.Length.ToString(), writer);
                    WriteNewLine(writer);
                    writer.Write(bulk.Content, 0, bulk.Content.Length);
                    WriteNewLine(writer);
                }
            }
            else if (token is ErrorRedisToken error)
            {
                WriteString("-" + error.Data, writer);
                WriteNewLine(writer);
            }
            else if (token is IntegerRedisToken num)
            {
                WriteString(":" + num.Data.ToString(), writer);
                WriteNewLine(writer);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void WriteNewLine(Stream writer)
        {

            writer.WriteByte((byte)'\r');
            writer.WriteByte((byte)'\n');
        }

        private void WriteString(string line, Stream writer)
        {
            var conten = Encoding.UTF8.GetBytes(line);
            writer.Write(conten, 0, conten.Length);
        }
    }
}
