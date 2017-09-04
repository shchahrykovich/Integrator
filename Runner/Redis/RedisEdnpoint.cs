using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Redis;
using Runner.AzureBlobService;

namespace Runner.Redis
{
    public class RedisEdnpoint: ProtocolEndpoint<RedisEndpointSettings>
    {
        private RedisHost _host;

        public RedisEdnpoint(CancellationToken token, RedisEndpointSettings settings) :base(token, settings)
        {
        }

        public override TestExecutionStats GetStats()
        {
            return new TestExecutionStats();
        }

        public override void PrintSettings(TextWriter log)
        {
            log.WriteLine(Settings.Name + " - localhost:" + Settings.Port);
        }

        public override void Start()
        {
            _host = new RedisHost(Token, Settings.Port, new RedisEngine(Settings.Port));
            _host.Start();
        }

        public override void Stop()
        {
            _host?.Stop();
        }
    }
}
