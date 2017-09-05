using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Amqp;
using Amqp.Framing;
using Amqp.Listener;

namespace Runner.Amqp
{
    internal class IncomingLinkEndpoint : LinkEndpoint
    {
        private readonly TaskCompletionSource<bool> _end;
        private ConcurrentBag<AMQPMessage> _messages;
        private long _id;

        public IncomingLinkEndpoint(TaskCompletionSource<bool> end)
        {
            _end = end;
            _messages = new ConcurrentBag<AMQPMessage>();
        }

        public override void OnMessage(MessageContext messageContext)
        {
            // this can also be done when an async operation, if required, is done
            messageContext.Complete();
        }

        public override void OnFlow(FlowContext flowContext)
        {
            for (int i = 0; i < flowContext.Messages; i++)
            {
                AMQPMessage data;
                if(_messages.TryTake(out data))
                {
                    if (data.Stop.HasValue && data.Stop.Value)
                    {
                        _end.SetException(new StopTestSignalException());
                    }

                    var message = new Message(data.Body);
                    message.Properties = new Properties() { Subject = "Message" + Interlocked.Increment(ref this._id) };
                    flowContext.Link.SendMessage(message);

                    if (!data.Batched)
                    {
                        return;
                    }
                }
                else
                {
                    var message = new Message();
                    message.Properties = new Properties() { Subject = "Message" + Interlocked.Increment(ref this._id) };
                    flowContext.Link.SendMessage(message);
                }
            }
        }

        public override void OnDisposition(DispositionContext dispositionContext)
        {
        }

        internal void AddStub(AMQPMessage stub)
        {
            _messages.Add(stub);
        }
    }
}