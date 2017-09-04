using System;
using System.Collections.Generic;
using System.Text;
using Redis;

namespace Runner.Redis
{
    public class RedisEngine : GenericRedisEngine
    {
        public RedisEngine(int port) : base(port)
        {
        }
    }
}
