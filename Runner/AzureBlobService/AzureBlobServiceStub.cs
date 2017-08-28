using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Runner.AzureBlobService
{
    public class AzureBlobServiceStub : ProtocolEndpoint
    {
        private string _fullName;
        private CancellationToken _token;

        public AzureBlobServiceStub(string fullName, CancellationToken token)
        {
            _fullName = fullName;
            _token = token;
        }

        public override void PrintSettings()
        {
            
        }

        public override void Start()
        {
            
        }

        public override void Stop()
        {
            
        }
    }
}
