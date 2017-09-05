using System.Collections.Generic;

namespace Runner
{
    public class TestExecutionStats
    {
        public IProtocolEndpoint Endpoint { get; private set; }
        public List<Stub> MissingStubs { get; private set; }

        public TestExecutionStats(IProtocolEndpoint endpoint)
        {
            Endpoint = endpoint;
            MissingStubs = new List<Stub>();
        }

        public void AddMissingStubs(IEnumerable<Stub> stubs)
        {
            MissingStubs.AddRange(stubs);
        }
    }
}
