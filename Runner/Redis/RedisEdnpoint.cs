using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Runner.AzureBlobService;

namespace Runner.Redis
{
    public class RedisEdnpoint: ProtocolEndpoint<RedisEndpointSettings>
    {

        public RedisEdnpoint(CancellationToken token, RedisEndpointSettings settings) :base(token, settings)
        {
        }

        public override TestExecutionStats GetStats()
        {
            throw new NotImplementedException();
        }

        public override void PrintSettings(TextWriter log)
        {
            throw new NotImplementedException();
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
