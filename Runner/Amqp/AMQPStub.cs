using System;
using System.IO;
using System.Threading;
using Amqp.Listener;
using Runner.Serialization;

namespace Runner.Amqp
{
    public class AMQPStub : ProtocolEndpoint<AMQPStubSettings>
    {
        private ContainerHost _host;

        public AMQPStub(CancellationToken token, AMQPStubSettings settings):base(token, settings)
        {
        }

        public override void Start()
        {
            var incommingLink = new IncomingLinkEndpoint();

            foreach (var stub in FileSerializer.ReadStubs<AMQPMessage>(Settings.FolderPath))
            {
                incommingLink.AddStub(stub);
            }

            Uri uri = new Uri("amqp://127.0.0.1:" + Settings.Port);
            _host = new ContainerHost(uri);
            _host.Open();

            _host.RegisterMessageProcessor(uri.AbsolutePath, new MessageProcessor());
            _host.RegisterLinkProcessor(new LinkProcessor(incommingLink));
        }

        public override void Stop()
        {
            _host.Close();
        }

        public override void PrintSettings(TextWriter log)
        {
            if (null != _host)
            {
                var addr = _host.Listeners[0].Address;
                log.WriteLine(Settings.Name + $" - {addr.Scheme}://{addr.Host}{addr.Path}:{addr.Port}");
            }
        }

        public override TestExecutionStats GetStats()
        {
            return new TestExecutionStats();
        }
    }
}