using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Redis;
using Runner.AzureBlobService;

namespace Runner.Redis
{
    public class RedisEdnpoint: ProtocolEndpoint<RedisEndpointSettings>
    {
        private RedisHost _host;

        public RedisEdnpoint(RedisEndpointSettings settings) :base( settings)
        {
        }

        public override TestExecutionStats GetStats()
        {
            return new TestExecutionStats(this);
        }

        public override void PrintSettings(TextWriter log)
        {
            log.WriteLine(Settings.Name + " - localhost:" + Settings.Port);
        }

        public override Task StartInternalAsync()
        {
            _host = new RedisHost(Token, Settings.Port, new RedisEngine(Settings.Port));
            _host.Start();

            return Task.Run(() => WaitHandle.WaitAll(new WaitHandle[] { Token.WaitHandle }));
        }

        public override void Stop()
        {
            _host?.Stop();
        }
    }
}
