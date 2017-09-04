using Microsoft.AspNetCore.Hosting;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AzureEmu
{
    public class AzureHost
    {
        private IWebHost _host;

        public void Start(IBlobServiceEngine engine, int port)
        {
            _host = new WebHostBuilder()
               .UseKestrel(options =>
               {
                   options.Listen(IPAddress.Loopback, port);
               }).UseLibuv()
               .UseStartup<Startup>()
               .ConfigureServices(services => services.Add(new ServiceDescriptor(typeof(IBlobServiceEngine), engine)))
               .Build();

            _host.Start();
        }

        public void Stop()
        {
            _host?.StopAsync().Wait();
        }
    }
}
