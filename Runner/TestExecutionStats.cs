using System.Collections.Generic;

namespace Runner
{
    public class TestExecutionStats
    {
        public Dictionary<Stub, int> Executions { get; private set; }
        public IProtocolEndpoint Endpoint { get; }
        public List<Stub> MissingStubs { get; }

        public TestExecutionStats(IProtocolEndpoint endpoint)
        {
            Endpoint = endpoint;
            MissingStubs = new List<Stub>();
            Executions = new Dictionary<Stub, int>();
        }

        public void AddMissingStubs(IEnumerable<Stub> stubs)
        {
            MissingStubs.AddRange(stubs);
        }

        public void SetExecutions(Dictionary<Stub, int> executions)
        {
            Executions = executions;
        }
    }
}
