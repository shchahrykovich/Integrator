using System;
using System.Data.SqlClient.Tests;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using Microsoft.SqlServer.TDS.Servers;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Runner
{
    internal class TDSStub : ProtocolEndpoint
    {
        private readonly string _folder;
        private readonly CancellationToken _token;
        private TestTdsServer _server;
        private JsonSerializer _json;

        public TDSStub(string folder, CancellationToken token)
        {
            _folder = folder;
            _token = token;
            _json = JsonSerializer.CreateDefault();
        }

        public override void Start()
        {

            var settings = ReaderSettings();

            var arguments = new TDSServerArguments { Log = Console.Out };
            var engine = new StaticQueryEngine(arguments);
            engine.Name = Path.GetFileName(_folder);

            Deserializer yamlDesirializer = new Deserializer();
            foreach (var stubFiles in Directory.GetFiles(_folder, "*.yml"))
            {
                using (var text = new StringReader(File.ReadAllText(stubFiles)))
                {
                    var sqlStub = yamlDesirializer.Deserialize<SqlStub>(text);
                    sqlStub.FileName = stubFiles;
                    sqlStub.Query = sqlStub.Query.TrimEnd().Replace("\n", "\r\n");

                    engine.AddStub(sqlStub);
                }
            }

            _server = TestTdsServer.StartTestServer(engine, port: settings.Port);
        }

        private TDSStubSettings ReaderSettings()
        {
            using (var text = new StringReader(File.ReadAllText(Path.Combine(_folder, "parameters.json"))))
            {
                using (var reader = new JsonTextReader(text))
                {
                    return _json.Deserialize<TDSStubSettings>(reader);
                }
            }
        }

        public override void Stop()
        {
            if (null != _server)
            {
                _server.Dispose();
                _server = null;
            }
        }

        public override void PrintSettings()
        {
            if (null != _server)
            {
                Console.WriteLine(Path.GetDirectoryName(_folder));
                Console.WriteLine(_server.ConnectionString);
            }
        }
    }
}