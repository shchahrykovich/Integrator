using System;

namespace Runner.Amqp
{
    public class AMQPMessage : Stub
    {
        public String Body { get; set; }

        public bool Batched { get; set; }

        public bool? Stop { get; set; }

        public int? Count { get; set; }

        public int GetCount()
        {
            if (Count.HasValue)
            {
                return Count.Value;
            }
            else
            {
                return 1;
            }
        }
    }
}
