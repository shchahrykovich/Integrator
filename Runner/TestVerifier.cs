using System;
using System.Collections.Generic;

namespace Runner
{
    internal class TestVerifier
    {
        private readonly IEnumerable<TestExecutionStats> _stats;

        public TestVerifier(IEnumerable<TestExecutionStats> stats)
        {
            _stats = stats;
        }

        public void VerifyAll()
        {
            foreach (var stat in _stats)
            {
            }
        }
    }
}