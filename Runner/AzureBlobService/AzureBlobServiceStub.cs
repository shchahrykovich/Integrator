using AzureEmu;
using AzureEmu.Engine;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Runner.Serialization;

namespace Runner.AzureBlobService
{
    public class AzureBlobServiceStub : ProtocolEndpoint
    {
        private string _folder;
        private CancellationToken _token;
        private AzureHost _host;
        private readonly AzureBlobServiceStubSettings _settings;

        public AzureBlobServiceStub(string folder, CancellationToken token, AzureBlobServiceStubSettings settings)
        {
            _folder = folder;
            _token = token;
            _settings = settings;
            _host = new AzureHost();
        }

        public override void PrintSettings()
        {
            if(null != _settings)
            {
                Console.WriteLine(Path.GetFileName(_folder) + " - http://localhost:" + _settings.Port);
            }
        }

        public override void Start()
        {
            var engine = new GenericBlobServiceEngine();
            foreach (var stub in FileSerializer.ReadStubs<BlobFileStub>(_folder))
            {
                var containerName = stub.GetContainerName();
                var blobName = stub.GetBlobName();
                engine.Add(containerName, blobName, stub.GetBytes());
            }

            _host.Start(engine, _settings.Port);
        }

        public override void Stop()
        {
            _host.Stop();
        }
    }
}