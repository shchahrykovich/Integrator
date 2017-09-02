using System;
using System.Collections.Generic;
using System.Text;

namespace Runner.Redis
{
    public class RedisEndpointSettings : ProtocolEndpointSettings
    {
        public int Port { get; set; }
    }
}
