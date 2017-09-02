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

                Test t = TestLoader.Load(test, Source.Token);
                TestRunner runner = new TestRunner(t, Source.Token, Console.Out);
                runner.Run();
            }
        }
    }
}
