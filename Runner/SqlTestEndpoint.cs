using System;
using System.Data.SqlClient.Tests;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using Microsoft.SqlServer.TDS.Servers;
using Newtonsoft.Json;

namespace Runner
{
    internal class SqlTestEndpoint : TestEndpoint
    {
        private readonly string _folder;
        private readonly CancellationToken _token;
        private TestTdsServer _server;
        private JsonSerializer _json;

        public SqlTestEndpoint(string folder, CancellationToken token)
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

            foreach (var queries in Directory.GetFiles(_folder, "data-*"))
            {
                using (var text = new StringReader(File.ReadAllText(queries)))
                {
                    using (var reader = new JsonTextReader(text))
                    {
                        var obj = _json.Deserialize<SqlTestData>(reader);
                        engine.AddTestData(obj);
                    }
                }
            }

            _server = TestTdsServer.StartTestServer(engine, port: settings.Port);
        }

        private SqlTestEndpointSettings ReaderSettings()
        {
            using (var text = new StringReader(File.ReadAllText(Path.Combine(_folder, "parameters.json"))))
            {
                using (var reader = new JsonTextReader(text))
                {
                    return _json.Deserialize<SqlTestEndpointSettings>(reader);
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