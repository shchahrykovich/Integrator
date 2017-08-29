using System;
using System.Collections.Generic;
using System.Text;

namespace Runner.Amqp
{
    public class AMQPMessage
    {
        public String Body { get; set; }
        [YamlDotNet.Serialization.YamlIgnore]
        public string FileName { get; internal set; }
        public bool Batched { get; set; }
    }
}
