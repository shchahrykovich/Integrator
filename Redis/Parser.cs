using Redis.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redis
{
    public class Parser
    {
        public Redis.Tokens.Token Parse(string line)
        {
            var result = ParseInternal(line, 0);
            return result;
        }

        private Token ParseInternal(string line, int index)
        {
            var tokenType = line[index + 0];
            switch (tokenType)
            {
                case '+':
                    {
                        return new SimpleString{
                            Data = line.Substring(1)
                        };
                    }
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
