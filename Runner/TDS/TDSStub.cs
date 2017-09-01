using System;
using System.IO;
using System.Threading;
using Microsoft.SqlServer.TDS.Servers;
using Runner.Serialization;

namespace Runner.TDS
{
    internal class TDSStub : ProtocolEndpoint
    {
        private readonly string _folder;
        private readonly CancellationToken _token;
        private readonly TDSStubSettings _settings;
        private TestTdsServer _server;

        public TDSStub(string folder, 
                       CancellationToken token, 
                       TDSStubSettings settings)
        {
            _folder = folder;
            _token = token;
            _settings = settings;
        }

        public override void Start()
        {
            var arguments = new TDSServerArguments {Log = Console.Out};
            var engine = new StaticQueryEngine(arguments);
            engine.Name = Path.GetFileName(_folder);

            foreach (var stub in FileSerializer.ReadStubs<SqlStub>(_folder))
            {
                engine.AddStub(stub);
            }

            _server = TestTdsServer.StartTestServer(engine, port: _settings.Port, enableLog: false);
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
                Console.WriteLine(Path.GetFileName(_folder) + " - " + _server.ConnectionString);
            }
        }
    }
}