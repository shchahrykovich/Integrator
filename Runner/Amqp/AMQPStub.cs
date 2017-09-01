using System;
using System.IO;
using System.Threading;
using Amqp.Listener;
using Runner.Serialization;

namespace Runner.Amqp
{
    public class AMQPStub : ProtocolEndpoint
    {
        private readonly string _folder;
        private readonly CancellationToken _token;
        private readonly AMQPStubSettings _settings;
        private ContainerHost _host;

        public AMQPStub(string folder, CancellationToken token, AMQPStubSettings settings)
        {
            _folder = folder;
            _token = token;
            _settings = settings;
        }

        public override void Start()
        {
            var incommingLink = new IncomingLinkEndpoint();

            foreach (var stub in FileSerializer.ReadStubs<AMQPMessage>(_folder))
            {
                incommingLink.AddStub(stub);
            }

            Uri uri = new Uri("amqp://127.0.0.1:" + _settings.Port);
            _host = new ContainerHost(uri);
            _host.Open();

            _host.RegisterMessageProcessor(uri.AbsolutePath, new MessageProcessor());
            _host.RegisterLinkProcessor(new LinkProcessor(incommingLink));
        }

        public override void Stop()
        {
            _host.Close();
        }

        public override void PrintSettings()
        {
            if (null != _host)
            {
                var addr = _host.Listeners[0].Address;
                Console.WriteLine(Path.GetFileName(_folder) + $" - {addr.Scheme}://{addr.Host}{addr.Path}:{addr.Port}");
            }
        }
    }
}