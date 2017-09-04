using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Runner.Serialization;

namespace Runner
{
    public class TestRunner
    {
        private readonly Test _test;
        private readonly CancellationToken _token;
        private readonly TextWriter _log;

        public TestRunner(Test test, CancellationToken token, TextWriter log)
        {
            _test = test;
            _token = token;
            _log = log;
        }

        public void Run()
        {
            _log.WriteLine("---------------------------------------");
            _log.WriteLine("Executing - " + _test.Name);
            _log.WriteLine("---------------------------------------");

            try
            {
                StartEndpoints();
                PrintSettings();
                Wait();
                StopEndpoints();
                CreateMissingStubs();
            }
            finally 
            {
                StopEndpoints();
            }
        }

        private void Wait()
        {
            if (!String.IsNullOrWhiteSpace(_test.Cmd))
            {
                var args = _test.Args ?? new string[0];
                ProcessStartInfo info = new ProcessStartInfo(_test.Cmd, String.Join(" ", args));
                info.RedirectStandardInput = true;
                info.WorkingDirectory = _test.WorkingDir ?? String.Empty;
                var p = Process.Start(info);
                using (_token.Register(() => { p.Kill(); }))
                {
                    p.WaitForExit();
                    if (0 != p.ExitCode)
                    {
                        Debugger.Break();
                    }
                }
            }
            else
            {
                WaitHandle.WaitAll(new WaitHandle[] {_token.WaitHandle});
            }
        }

        private void PrintSettings()
        {
            foreach (var endpoint in _test.Endpoints)
            {
                endpoint.PrintSettings(_log);
            }
        }

        private void StartEndpoints()
        {
            foreach (var endpoint in _test.Endpoints)
            {
                endpoint.Start();
            }
        }

        private void CreateMissingStubs()
        {
            foreach (var endpoint in _test.Endpoints)
            {
                TestExecutionStats stats = endpoint.GetStats();
                foreach (Stub stub in stats.MissingStubs)
                {
                    var path = Path.Combine(endpoint.Settings.FolderPath, "_Missing", $"{stub.Name}.yml");
                    FileSerializer.WriteStub(path, stub);
                }
            }
        }

        private void StopEndpoints()
        {
            foreach (var endpoint in _test.Endpoints)
            {
                endpoint.Stop();
            }
        }
    }
}
