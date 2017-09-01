using Amqp.Listener;

namespace Runner.Amqp
{
    internal class LinkProcessor : ILinkProcessor
    {
        private IncomingLinkEndpoint _incommingLink;

        public LinkProcessor(IncomingLinkEndpoint incommingLink)
        {
            _incommingLink = incommingLink;
        }

        public void Process(AttachContext attachContext)
        {
            attachContext.Complete(_incommingLink, 300);
        }
    }
}