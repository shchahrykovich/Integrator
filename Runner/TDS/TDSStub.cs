using System;
using System.IO;
using System.Threading;
using Microsoft.SqlServer.TDS.Servers;
using Runner.Serialization;

namespace Runner.TDS
{
    internal class TDSStub : ProtocolEndpoint<TDSStubSettings>
    {
        private TestTdsServer _server;
        private StaticQueryEngine _engine;

        public TDSStub(CancellationToken token, 
                       TDSStubSettings settings): base(token, settings)
        {
        }

        public override void Start()
        {
            var arguments = new TDSServerArguments {Log = Console.Out};
            _engine = new StaticQueryEngine(arguments);
            _engine.Name = Settings.Name;

            foreach (var stub in FileSerializer.ReadStubs<SqlStub>(Settings.FolderPath))
            {
                _engine.AddStub(stub);
            }

            _server = TestTdsServer.StartTestServer(_engine, port: Settings.Port, enableLog: false);
        }

        public override void Stop()
        {
            if (null != _server)
            {
                _server.Dispose();
                _server = null;
            }
        }

        public override void PrintSettings(TextWriter log)
        {
            if (null != _server)
            {
                log.WriteLine(Settings.Name + " - " + _server.ConnectionString);
            }
        }

        public override TestExecutionStats GetStats()
        {
            TestExecutionStats stats = new TestExecutionStats();
            stats.AddMissingStubs(_engine.GetMissingStubs());

            return stats;
        }
    }
}