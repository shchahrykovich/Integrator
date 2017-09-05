using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Runner.Serialization;

namespace Runner
{
    class Program
    {
        private static CancellationTokenSource Source = new CancellationTokenSource();

        static void Main(string[] args)
        {
            try
            {
                RunApp(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Environment.ExitCode = 1;
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
        }

        private static void RunApp(string[] args)
        {
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                Source.Cancel();
            };

            if (0 == args.Length)
            {
                Console.WriteLine("Commands:");
                Console.WriteLine("\t{folder} - run all tests fromt specified folder");
                return;
            }

            var tests = args[0];

            foreach (var test in Directory.GetDirectories(tests, "*", SearchOption.TopDirectoryOnly))
            {
                if (Path.GetFileName(test).StartsWith("!") || Path.GetFileName(test).StartsWith("."))
                {
                    continue;
                }

                Test t = TestLoader.Load(test);
                TestRunner runner = new TestRunner(t, Source.Token, Console.Out);
                var stats = runner.Run().ToArray();
                CreateMissingStubs(stats);
                var verifier = new TestVerifier(stats);
                verifier.VerifyAll();
            }
        }

        private static void CreateMissingStubs(IEnumerable<TestExecutionStats> array)
        {
            foreach (var stats in array)
            {
                foreach (var stub in stats.MissingStubs)
                {
                    var path = Path.Combine(stats.Endpoint.FolderPath, "_Missing", $"{stub.Name}.yml");
                    FileSerializer.WriteStub(path, stub);
                }
            }
        }
    }
}
