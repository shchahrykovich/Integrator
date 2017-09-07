using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amqp.Listener;
using Runner.Serialization;

namespace Runner.Amqp
{
    public class AMQPStub : ProtocolEndpoint<AMQPStubSettings>
    {
        private ContainerHost _host;
        private CancellationTokenRegistration _cancellationRegistration;
        private List<AMQPMessage> _messages = new List<AMQPMessage>();

        public AMQPStub(AMQPStubSettings settings, IEnumerable<AMQPMessage> stubs): base(settings)
        {
            foreach (var stub in stubs)
            {
                for (int i = 0; i < stub.GetCount(); i++)
                {
                    _messages.Add(stub);
                }
            }
        }

        public override Task StartInternalAsync()
        {
            var end = new TaskCompletionSource<bool>();
            _cancellationRegistration = Token.Register(() =>
            {
                end.TrySetCanceled();
            });

            var incommingLink = new IncomingLinkEndpoint(end, _messages);

            var uri = new Uri("amqp://localhost:" + Settings.Port);
            _host = new ContainerHost(uri);

            _host.Open();

            _host.RegisterMessageProcessor(uri.AbsolutePath, new MessageProcessor());
            _host.RegisterLinkProcessor(new LinkProcessor(incommingLink));

            return end.Task;
        }

        public override IEnumerable<Stub> GetAllStubs()
        {
            return Enumerable.Empty<Stub>();
        }

        public override void Stop()
        {
            _cancellationRegistration.Dispose();
            _host?.Close();
        }

        public override void PrintSettings(TextWriter log)
        {
            if (null != _host)
            {
                var addr = _host.Listeners[0].Address;
                log.WriteLine(Settings.Name + $" - {addr.Scheme}://localhost{addr.Path}:{addr.Port}");
            }
        }

        public override TestExecutionStats GetStats()
        {
            return new TestExecutionStats(this);
        }
    }
}