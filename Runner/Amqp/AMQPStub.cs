using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Amqp.Listener;
using Queue;
using Newtonsoft.Json;
using System.IO;
using Runner.Amqp;
using YamlDotNet.Serialization;

namespace Runner
{
    public class AMQPStub : ProtocolEndpoint
    {
        private readonly string _folder;
        private readonly CancellationToken _token;
        private readonly JsonSerializer _json;
        private ContainerHost _host;

        public AMQPStub(string folder, CancellationToken token)
        {
            _folder = folder;
            _token = token;
            _json = JsonSerializer.CreateDefault();
        }

        public override void Start()
        {
            var settings = ReaderSettings();

            var incommingLink = new IncomingLinkEndpoint();
            Deserializer yamlDesirializer = new Deserializer();
            foreach (var stubFile in Directory.GetFiles(_folder, "*.yml"))
            {
                using (var text = new StringReader(File.ReadAllText(stubFile)))
                {
                    var stub = yamlDesirializer.Deserialize<AMQPMessage>(text);
                    stub.FileName = stubFile;
                    stub.Body = stub.Body.TrimEnd().Replace("\n", "\r\n");

                    incommingLink.AddStub(stub);
                }
            }

            Uri uri = new Uri("amqp://127.0.0.1:" + settings.Port);
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
            var addr = _host.Listeners[0].Address;
            Console.WriteLine($"Amqp {addr.Scheme}://{addr.Host}{addr.Path}:{addr.Port}");
        }

        private AMQPStubSettings ReaderSettings()
        {
            var parameterFileName = Path.Combine(_folder, "parameters.json");
            if (!File.Exists(parameterFileName))
            {
                return new AMQPStubSettings
                {
                    Port = 54330
                };
            }
            using (var text = new StringReader(File.ReadAllText(parameterFileName)))
            {
                using (var reader = new JsonTextReader(text))
                {
                    return _json.Deserialize<AMQPStubSettings>(reader);
                }
            }
        }
    }
}