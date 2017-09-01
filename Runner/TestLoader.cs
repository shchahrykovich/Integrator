using System;
using System.IO;
using System.Threading;
using Runner.Amqp;
using Runner.AzureBlobService;
using Runner.Serialization;
using Runner.TDS;

namespace Runner
{
    public static class TestLoader
    {
        public static Test Load(string path, CancellationToken token)
        {
            var test = FileSerializer.ReadConfig<Test>(path, new Test());
            test.Name = Path.GetFileName(path);
            test.Folder = path;

            var testDir = new DirectoryInfo(test.Folder);
            foreach (var endpointDir in testDir.GetDirectories("*", SearchOption.TopDirectoryOnly))
            {
                if (Is(endpointDir, "Sql"))
                {
                    var sqlSettings = FileSerializer.ReadConfig<TDSStubSettings>(endpointDir.FullName, new TDSStubSettings{Port = 54300});
                    test.AddEndpoint(new TDSStub(endpointDir.FullName, token, sqlSettings));
                }
                else if (Is(endpointDir, "Amqp"))
                {
                    var amqpSettings = FileSerializer.ReadConfig<AMQPStubSettings>(endpointDir.FullName, new AMQPStubSettings{Port = 54400});
                    test.AddEndpoint(new AMQPStub(endpointDir.FullName, token, amqpSettings));
                }
                else if (Is(endpointDir, "AzureBlobService"))
                {
                    var blobSettings = FileSerializer.ReadConfig<AzureBlobServiceStubSettings>(endpointDir.FullName,
                        new AzureBlobServiceStubSettings {Port = 54500});
                    test.AddEndpoint(new AzureBlobServiceStub(endpointDir.FullName, token, blobSettings));
                }
            }

            return test;
        }

        private static bool Is(DirectoryInfo endpointDir, string name)
        {
            return endpointDir.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
