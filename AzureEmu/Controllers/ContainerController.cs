using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace AzureEmu.Controllers
{
    public class ContainerController : ControllerBase
    {
        private readonly IBlobServiceEngine _engine;

        public ContainerController(IBlobServiceEngine engine)
        {
            _engine = engine;
        }

        [HttpGet]
        public string Get(String container)
        {
            throw new NotImplementedException();
        }

        [HttpPut]
        public IActionResult Put(String container, String restype)
        {
            if (_engine.ContainsContainer(container))
            {
                HttpContext.Response.Headers.Add("x-ms-version", "2017-04-17");

                var result = new ContentResult
                {
                    StatusCode = (int) HttpStatusCode.Conflict,
                    ContentType = "application/xml",
                    Content =
                        @"<?xml version=""1.0"" encoding=""utf-8"" ?><Error><Code>ContainerAlreadyExists</Code><Message>The specified container already exists.
            RequestId:" + Guid.NewGuid().ToString("D") + @"
            Time:" + DateTime.UtcNow.ToString("o") + "</Message></Error>"
                };

                return result;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
