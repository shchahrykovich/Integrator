using Amqp.Listener;

namespace Runner.Amqp
{
    public class MessageProcessor : IMessageProcessor
    {
        public int Credit => throw new System.NotImplementedException();

        public void Process(MessageContext messageContext)
        {
            throw new System.NotImplementedException();
        }
    }
}