using System;
using System.Collections.Generic;
using System.Text;

namespace Runner.Amqp
{
    public class AMQPMessage
    {
        public String Body { get; set; }
        public string FileName { get; internal set; }
    }
}
