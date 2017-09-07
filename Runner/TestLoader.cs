using System;
using System.IO;
using System.Linq;
using System.Threading;
using Runner.Amqp;
using Runner.AzureBlobService;
using Runner.Serialization;
using Runner.TDS;
using Runner.Redis;

namespace Runner
{
    public static class TestLoader
    {
        public static Test Load(string path)
        {
            var test = FileSerializer.ReadConfig<Test>(path, new Test());

            var testDir = new DirectoryInfo(Path.Combine(test.FolderPath, @".\..\_Endpoints\"));
            foreach (var endpointDir in testDir.GetDirectories("*", SearchOption.TopDirectoryOnly))
            {
                if (Is(endpointDir, "Sql-"))
                {
                    var sqlSettings = FileSerializer.ReadConfig<TDSStubSettings>(endpointDir.FullName, new TDSStubSettings{Port = 54300});
                    var stubs = FileSerializer.ReadStubs<SqlStub>(endpointDir.FullName).ToList();
                    var dirPath = Path.Combine(test.FolderPath, endpointDir.Name);
                    if (Directory.Exists(dirPath))
                    {
                        stubs.AddRange(FileSerializer.ReadStubs<SqlStub>(dirPath));
                    }
                    test.AddEndpoint(new TDSStub(sqlSettings, stubs));
                }
                else if (Is(endpointDir, "Amqp-"))
                {
                    var amqpSettings = FileSerializer.ReadConfig<AMQPStubSettings>(endpointDir.FullName, new AMQPStubSettings{Port = 54400});
                    var stubs = FileSerializer.ReadStubs<AMQPMessage>(endpointDir.FullName).ToList();
                    var dirPath = Path.Combine(test.FolderPath, endpointDir.Name);
                    if (Directory.Exists(dirPath))
                    {
                        stubs.AddRange(FileSerializer.ReadStubs<AMQPMessage>(dirPath));
                    }
                    test.AddEndpoint(new AMQPStub(amqpSettings, stubs));
                }
                else if (Is(endpointDir, "AzureBlobService-"))
                {
                    var blobSettings = FileSerializer.ReadConfig<AzureBlobServiceStubSettings>(endpointDir.FullName,
                        new AzureBlobServiceStubSettings {Port = 54500});
                    var stubs = FileSerializer.ReadStubs<BlobFileStub>(endpointDir.FullName).ToList();
                    var dirPath = Path.Combine(test.FolderPath, endpointDir.Name);
                    if (Directory.Exists(dirPath))
                    {
                        stubs.AddRange(FileSerializer.ReadStubs<BlobFileStub>(dirPath));
                    }
                    test.AddEndpoint(new AzureBlobServiceStub(blobSettings, stubs));
                }
                else if (Is(endpointDir, "Redis-"))
                {
                    var settings = FileSerializer.ReadConfig<RedisEndpointSettings>(endpointDir.FullName,
                        new RedisEndpointSettings { Port = 54600 });
                    test.AddEndpoint(new RedisEdnpoint(settings));
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
