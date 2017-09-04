using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Redis.Tokens
{
    [DebuggerDisplay("{Data}")]
    public class SimpleStringRedisToken : RedisToken
    {
        public String Data { get; set; }

        public override object GetData()
        {
            return Data;
        }
    }
}
