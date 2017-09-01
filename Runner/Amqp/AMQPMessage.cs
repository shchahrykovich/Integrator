using System;

namespace Runner.Amqp
{
    public class AMQPMessage : Stub
    {
        public String Body { get; set; }

        public bool Batched { get; set; }
    }
}
