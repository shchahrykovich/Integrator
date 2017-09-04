using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Redis.Tokens
{
    [DebuggerDisplay("{Data}")]
    public class BulkStringRedisToken : RedisToken
    {
        private string _data;

        public string Data
        {
            get { return _data; }
            set
            {
                _data = value;
                _content = Encoding.UTF8.GetBytes(_data);
            }
        }

        private byte[] _content;

        public byte[] Content
        {
            get { return _content; }
            set
            {
                _content = value;
                _data = Encoding.UTF8.GetString(_content);
            }
        }

        public override object GetData()
        {
            return Data;
        }
    }
}
