using AzureEmu;
using AzureEmu.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using YamlDotNet.Serialization;

namespace Runner.AzureBlobService
{
    public class AzureBlobServiceStub : ProtocolEndpoint
    {
        private string _folder;
        private CancellationToken _token;
        private AzureHost _host;
        private Deserializer _yamlDesirializer;
        private AzureBlobServiceStubSettings _settings;

        public AzureBlobServiceStub(string folder, CancellationToken token)
        {
            _folder = folder;
            _token = token;
            _host = new AzureHost();
            _yamlDesirializer = new Deserializer();
        }

        public override void PrintSettings()
        {
            if(null != _settings)
            {
                Console.WriteLine("Blob service http://localhost:" + _settings.Port);
            }
        }

        public override void Start()
        {
            _settings = ReaderSettings();

            var engine = new GenericBlobServiceEngine();
            foreach (var stubFilePath in Directory.GetFiles(_folder, "*.yml", SearchOption.AllDirectories))
            {
                if (stubFilePath != Path.Combine(_folder, "parameters.yml"))
                {
                    using (var text = new StringReader(File.ReadAllText(stubFilePath)))
                    {
                        var stub = _yamlDesirializer.Deserialize<BlobFileStub>(text);
                        stub.Content = stub.Content.TrimEnd().Replace("\n", "\r\n");

                        var relativePath = stubFilePath.Substring(_folder.Length + 1);
                        var containerName = relativePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).First();

                        var blobName = stubFilePath.Substring(_folder.Length + containerName.Length + 1).Replace(Path.DirectorySeparatorChar, '/');
                        blobName = blobName.Replace(".yml", "");
                        engine.Add(containerName, blobName, Encoding.UTF8.GetBytes(stub.Content));
                    }
                }
            }

            _host.Start(engine, _settings.Port);
        }

        private AzureBlobServiceStubSettings ReaderSettings()
        {
            var parameterFileName = Path.Combine(_folder, "parameters.yml");
            if (!File.Exists(parameterFileName))
            {
                return new AzureBlobServiceStubSettings
                {
                    Port = 55330
                };
            }
            return _yamlDesirializer.Deserialize<AzureBlobServiceStubSettings>(File.ReadAllText(parameterFileName));
        }

        public override void Stop()
        {
            _host.Stop();
        }
    }
}