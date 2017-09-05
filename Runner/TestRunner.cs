using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Runner
{
    public class TestRunner
    {
        private readonly Test _test;
        private readonly CancellationToken _token;
        private readonly CancellationTokenSource _stopExecution = new CancellationTokenSource();
        private readonly TextWriter _log;

        public TestRunner(Test test, CancellationToken token, TextWriter log)
        {
            _test = test;
            _token = CancellationTokenSource.CreateLinkedTokenSource(_stopExecution.Token, token).Token;
            _log = log;
        }

        public IEnumerable<TestExecutionStats> Run()
        {
            _log.WriteLine("---------------------------------------");
            _log.WriteLine("Executing - " + _test.Name);
            _log.WriteLine("---------------------------------------");
            try
            {
                var endpoints = StartEndpoints(_token);
                PrintSettings();
                var main = Task.Run(() => RunApp());

                Task.WaitAny(endpoints);
                _stopExecution.Cancel();
                try
                {
                    Task.WaitAll(endpoints);
                }
                catch (AggregateException ex)
                {
                    if (!(ex.InnerExceptions.All(e => e is StopTestSignalException) ||
                        ex.InnerExceptions.All(e => e is TaskCanceledException)))
                    {
                        throw;
                    }
                }

                main.Wait();
            }
            finally
            {
                StopEndpoints();
            }

            return _test.Endpoints.Select(e => e.GetStats()).ToArray();
        }

        private void StopEndpoints()
        {
            foreach (var endpoint in _test.Endpoints)
            {
                endpoint.Stop();
            }
        }

        private int RunApp()
        {
            if (!String.IsNullOrWhiteSpace(_test.Cmd))
            {
                var args = _test.Args ?? new string[0];
                ProcessStartInfo info = new ProcessStartInfo(_test.Cmd, String.Join(" ", args));
                info.RedirectStandardInput = true;
                info.WorkingDirectory = _test.WorkingDir ?? String.Empty;
                var p = Process.Start(info);

                var handle = new ProcessWaitHandle(p);
                var index = WaitHandle.WaitAny(new WaitHandle[] {_token.WaitHandle, handle});
                if (index == 0)
                {
                    p.Kill();
                }
                else
                {
                    _stopExecution.Cancel();
                }
                return p.ExitCode;
            }
            else
            {
                WaitHandle.WaitAll(new WaitHandle[] {_token.WaitHandle});
            }
            return 0;
        }

        private void PrintSettings()
        {
            foreach (var endpoint in _test.Endpoints)
            {
                endpoint.PrintSettings(_log);
            }
        }

        private Task[] StartEndpoints(CancellationToken token)
        {
            List<Task> tasks = new List<Task>();

            foreach (var endpoint in _test.Endpoints)
            {
                tasks.Add(endpoint.StartAsync(token));
            }
            return tasks.ToArray();
        }
    }
}
