using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlClient.Tests;
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

                Test t = new Test();
                var configFile = Path.Combine(test, "parameters.json");
                if (File.Exists(configFile))
                {
                    var json = JsonSerializer.CreateDefault();
                    using (var text = new StringReader(File.ReadAllText(configFile)))
                    {
                        using (var reader = new JsonTextReader(text))
                        {
                            t = json.Deserialize<Test>(reader);
                        }
                    }
                }

                List<TestEndpoint> endpoints = new List<TestEndpoint>();

                var rootDir = new DirectoryInfo(test);
                foreach (var endpointDir in rootDir.GetDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    if (endpointDir.Name.StartsWith("Sql"))
                    {
                        endpoints.Add(new SqlTestEndpoint(endpointDir.FullName, Source.Token));
                    }
                }

                foreach (var endpoint in endpoints)
                {
                    endpoint.Start();
                }

                foreach (var endpoint in endpoints)
                {
                    endpoint.PrintSettings();
                }

                if (!String.IsNullOrWhiteSpace(t.Cmd))
                {
                    var p = Process.Start(t.Cmd, String.Join(" ", t.Args));
                    p.WaitForExit();
                    if(0 != p.ExitCode)
                    {
                        Debugger.Break();
                    }
                }

                foreach (var endpoint in endpoints)
                {
                    endpoint.Stop();
                }
            }

            //WaitHandle.WaitAll(new WaitHandle[] {Source.Token.WaitHandle});
        }
    }
}
