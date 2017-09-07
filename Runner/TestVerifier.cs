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

        public IEnumerable<AssertFail> VerifyAll()
        {
            foreach (var stat in _stats)
            {
                foreach (var stub in stat.Endpoint.GetAllStubs())
                {
                    var verify = stub.Verify;
                    if (null == verify)
                    {
                        continue;
                    }
                    if (verify.AtLeast.HasValue)
                    {
                        if (stat.Executions.ContainsKey(stub))
                        {
                            var actual = stat.Executions[stub];
                            var expected = verify.AtLeast.Value;
                            if (actual < expected)
                            {
                                yield return new AssertFail(stub,
                                    $"Expected at least {expected} executions, actual {actual}.");
                            }
                        }
                        else
                        {
                            yield return new AssertFail(stub, "This stub hasn't been called.");
                        }
                    }
                    else if (verify.Exactly.HasValue)
                    {
                        if (verify.Exactly.Value == 0)
                        {
                            if (stat.Executions.ContainsKey(stub))
                            {
                                var actual = stat.Executions[stub];
                                yield return new AssertFail(stub, $"Expected 0 executions, actual {actual}.");
                            }
                        }
                        else
                        {
                            if (stat.Executions.ContainsKey(stub))
                            {
                                var actual = stat.Executions[stub];
                                var expected = verify.Exactly.Value;
                                if (actual != expected)
                                {
                                    yield return new AssertFail(stub,
                                        $"Expected {expected} executions, actual {actual}.");
                                }
                            }
                            else
                            {
                                yield return new AssertFail(stub, "This stub hasn't been called.");
                            }
                        }
                    }
                }
            }
        }
    }
}