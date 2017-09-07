using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SqlServer.TDS.Servers;
using Runner.Serialization;

namespace Runner.TDS
{
    internal class TDSStub : ProtocolEndpoint<TDSStubSettings>
    {
        private TestTdsServer _server;
        private StaticQueryEngine _engine;
        private readonly List<SqlStub> _exestingStubs = new List<SqlStub>();

        public TDSStub(TDSStubSettings settings, IEnumerable<SqlStub> stubs) : base(settings)
        {
            _exestingStubs.AddRange(stubs);
        }

        public override Task StartInternalAsync()
        {
            var arguments = new TDSServerArguments {Log = Console.Out};
            _engine = new StaticQueryEngine(arguments);
            foreach (var stub in _exestingStubs)
            {
                _engine.AddStub(stub);
            }
            _engine.Name = Settings.Name;

            _server = TestTdsServer.StartTestServer(_engine, port: Settings.Port, enableLog: false);

            return Task.Run(() => WaitHandle.WaitAll(new WaitHandle[] { Token.WaitHandle }));
        }

        public override IEnumerable<Stub> GetAllStubs()
        {
            foreach (var stub in _exestingStubs)
            {
                yield return stub;
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

        public override void PrintSettings(TextWriter log)
        {
            if (null != _server)
            {
                log.WriteLine(Settings.Name + " - " + _server.ConnectionString);
            }
        }

        public override TestExecutionStats GetStats()
        {
            TestExecutionStats stats = new TestExecutionStats(this);
            stats.AddMissingStubs(_engine.GetMissingStubs());
            stats.SetExecutions(_engine.GetExecutions());

            return stats;
        }
    }
}