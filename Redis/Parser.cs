using Redis.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Redis
{
    public class Parser
    {
        public RedisToken Parse(Stream reader)
        {
            var type = new byte[1];
            var readLen = reader.ReadAsync(type, 0, 1).Result;
            if (0 == readLen)
            {
                return null;
            }
            switch ((char) type[0])
            {
                case '*':
                {
                    var length = int.Parse(ReadLine(reader));
                    var token = new ArrayRedisToken(length);
                    for (int i = 0; i < length; i++)
                    {
                        token.Add(Parse(reader));
                    }
                    return token;
                }
                case '+':
                {
                    return new SimpleStringRedisToken
                    {
                        Data = ReadLine(reader)
                    };
                }
                case ':':
                {
                    return new IntegerRedisToken
                    {
                        Data = int.Parse(ReadLine(reader))
                    };
                }
                case '$':
                {
                    var length = int.Parse(ReadLine(reader));
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
                    ReadEndLine(reader);
                    return new BulkStringRedisToken()
                    {
                        Content = content

                    };
                }
                default:
                    throw new NotImplementedException();
            }
        }

        private void ReadEndLine(Stream reader)
        {
            if ('\r' != reader.ReadByte() || '\n' != reader.ReadByte())
            {
                throw  new ApplicationException("Can't read end line");
            }
        }

        private string ReadLine(Stream s)
        {
            List<byte> content = new List<byte>();

            var carriageReturn = (char)s.ReadByte();
            content.Add((byte)carriageReturn);

            var newLine = (char)s.ReadByte();
            content.Add((byte)newLine);

            while (carriageReturn != '\r' && newLine != '\n')
            {
                var current = (char) s.ReadByte();
                content.Add((byte)current);

                carriageReturn = newLine;
                newLine = current;
            }

            var result = Encoding.UTF8.GetString(content.ToArray());
            return result.Substring(0, result.Length -2);
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
