using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace AzureEmu.Tests
{
    public class RouterTests : IDisposable
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public RouterTests()
        {
            _server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            _client = _server.CreateClient();
        }

        [Theory]
        [InlineData("Put", "/container")]
        [InlineData("Get", "/container")]
        [InlineData("Get", "/")]
        [InlineData("Get", "/container/blob")]
        [InlineData("Head", "/container/blob")]
        public async Task Put_Request_Should_Return_Success(string method, string url)
        {
            // Act
            var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod(method), url));
                
            // Assert
            Assert.NotEqual(response.StatusCode, HttpStatusCode.NotFound);
        }

        public void Dispose()
        {
            _server?.Dispose();
            _client?.Dispose();
        }
    }
}
