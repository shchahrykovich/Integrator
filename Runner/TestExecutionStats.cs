using System.Collections.Generic;

namespace Runner
{
    public class TestExecutionStats
    {
        public List<Stub> MissingStubs { get; private set; }

        public TestExecutionStats()
        {
            MissingStubs = new List<Stub>();
        }

        public void AddMissingStubs(IEnumerable<Stub> stubs)
        {
            MissingStubs.AddRange(stubs);
        }
    }
}
