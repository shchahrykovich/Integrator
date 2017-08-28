using Newtonsoft.Json;
using Runner.AzureBlobService;
using System;
using System.Collections.Generic;
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

                Console.WriteLine("---------------------------------------");
                Console.WriteLine("Executing - " + Path.GetFileName(test));
                Console.WriteLine("---------------------------------------");

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

                List<ProtocolEndpoint> endpoints = new List<ProtocolEndpoint>();

                var rootDir = new DirectoryInfo(test);
                foreach (var endpointDir in rootDir.GetDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    if (endpointDir.Name.StartsWith("Sql"))
                    {
                        endpoints.Add(new TDSStub(endpointDir.FullName, Source.Token));
                    }
                    else if (endpointDir.Name.StartsWith("Amqp"))
                    {
                        endpoints.Add(new AMQPStub(endpointDir.FullName, Source.Token));
                    }
                    else if (endpointDir.Name.StartsWith("AzureBlobService"))
                    {
                        endpoints.Add(new AzureBlobServiceStub(endpointDir.FullName, Source.Token));
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
                    if (0 != p.ExitCode)
                    {
                        Debugger.Break();
                    }
                }
                else
                {
                    WaitHandle.WaitAll(new WaitHandle[] { Source.Token.WaitHandle });
                }

                foreach (var endpoint in endpoints)
                {
                    endpoint.Stop();
                }
            }
        }
    }
}
