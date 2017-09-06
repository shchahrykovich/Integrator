using Redis.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Redis
{
    public class Parser
    {
        public RedisToken Parse(RedisStreamReader reader)
        {
            var rawType = reader.ReadByte();
            if (!rawType.HasValue)
            {
                return null;
            }
            var type = (char) rawType.Value;
            switch (type)
            {
                case '*':
                {
                    var length = int.Parse(reader.ReadLine());
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
                        Data = reader.ReadLine()
                    };
                }
                case ':':
                {
                    return new IntegerRedisToken
                    {
                        Data = int.Parse(reader.ReadLine())
                    };
                }
                case '$':
                {
                    var length = int.Parse(reader.ReadLine());
                    var content = reader.ReadBytes(length);
                    reader.ReadEndLine();
                    return new BulkStringRedisToken()
                    {
                        Content = content
                    };
                }
                default:
                    throw new NotImplementedException();
            }
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
            writer.WriteByte((byte) '\r');
            writer.WriteByte((byte) '\n');
        }

        private void WriteString(string line, Stream writer)
        {
            var conten = Encoding.UTF8.GetBytes(line);
            writer.Write(conten, 0, conten.Length);
        }
    }
}