using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;

namespace Runner.Amqp
{
    public class AMQPMessage
    {
        [YamlMember(Alias = "body")]
        public String Body { get; set; }

        [YamlIgnore]
        public string FileName { get; internal set; }

        [YamlMember(Alias = "batched")]
        public bool Batched { get; set; }
    }
}
