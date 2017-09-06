using AzureEmu;
using AzureEmu.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Runner.Serialization;

namespace Runner.AzureBlobService
{
    public class AzureBlobServiceStub : ProtocolEndpoint<AzureBlobServiceStubSettings>
    {
        private AzureHost _host;

        public AzureBlobServiceStub(AzureBlobServiceStubSettings settings) : base(settings)
        {
            _host = new AzureHost();
        }

        public override void PrintSettings(TextWriter log)
        {
            log.WriteLine(Settings.Name + " - http://localhost:" + Settings.Port);
        }

        public override TestExecutionStats GetStats()
        {
            return new TestExecutionStats(this);
        }

        public override Task StartInternalAsync()
        {
            var engine = new GenericBlobServiceEngine();
            foreach (var stub in FileSerializer.ReadStubs<BlobFileStub>(Settings.FolderPath))
            {
                var containerName = stub.GetContainerName();
                var blobName = stub.GetBlobName();
                engine.Add(containerName, blobName, stub.GetBytes());
            }

            _host.Start(engine, Settings.Port);

            return Task.Run(() => WaitHandle.WaitAll(new WaitHandle[] {Token.WaitHandle}));
        }

        public override IEnumerable<Stub> GetAllStubs()
        {
            return Enumerable.Empty<Stub>();
        }

        public override void Stop()
        {
            _host.Stop();
        }
    }
}