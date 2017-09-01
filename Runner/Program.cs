using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Runner
{
    class Program
    {
        private static CancellationTokenSource Source = new CancellationTokenSource();

        static void Main(string[] args)
        {
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                Source.Cancel();
            };

            if (0 == args.Length)
            {
                Console.WriteLine("No arguments.");
                return;
            }

            var tests = args[0];

            foreach (var test in Directory.GetDirectories(tests, "*", SearchOption.TopDirectoryOnly))
            {
                if (Path.GetFileName(test).StartsWith("!"))
                {
                    continue;
                }

                Test t = TestLoader.Load(test, Source.Token);

                Console.WriteLine("---------------------------------------");
                Console.WriteLine("Executing - " + Path.GetFileName(t.Name));
                Console.WriteLine("---------------------------------------");
                
                foreach (var endpoint in t.Endpoints)
                {
                    endpoint.Start();
                }

                foreach (var endpoint in t.Endpoints)
                {
                    endpoint.PrintSettings();
                }

                if (!String.IsNullOrWhiteSpace(t.Cmd))
                {
                    var p = Process.Start(t.Cmd, String.Join(" ", t.Args));
                    p.WaitForExit();
                    if (0 != p.ExitCode)
                    {
                        Debugger.Break();
                    }
                }
                else
                {
                    WaitHandle.WaitAll(new WaitHandle[] { Source.Token.WaitHandle });
                }

                foreach (var endpoint in t.Endpoints)
                {
                    endpoint.Stop();
                }
            }
        }
    }
}
