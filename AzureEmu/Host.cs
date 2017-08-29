using Microsoft.AspNetCore.Hosting;
using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace AzureEmu
{
    public class AzureHost
    {
        public void Start(IBlobServiceEngine engine, int port)
        {
            var host = new WebHostBuilder()
               .UseKestrel(options =>
               {
                   options.Listen(IPAddress.Loopback, port);
               }).UseLibuv()
               .UseStartup<Startup>()
               .ConfigureServices(services => services.Add(new ServiceDescriptor(typeof(IBlobServiceEngine), engine)))
               .Build();

            host.Run();
        }
    }
}
