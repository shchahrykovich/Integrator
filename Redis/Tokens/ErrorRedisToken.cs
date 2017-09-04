using System;
using System.Collections.Generic;
using System.Text;

namespace Redis.Tokens
{
    public class ErrorRedisToken : RedisToken
    {
        public String Data { get; set; }

        public override object GetData()
        {
            return Data;
        }
    }
}
