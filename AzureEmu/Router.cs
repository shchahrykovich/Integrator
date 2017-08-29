using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureEmu
{
    public class Router : IRouter
    {
        private readonly IRouter _defaultHandler;

        public Router(IRouter defaultHandler)
        {
            _defaultHandler = defaultHandler;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            return null;
        }

        public async Task RouteAsync(RouteContext context)
        {
            var request = context.HttpContext.Request;
            var path = request.Path;
            var parts = path.ToString().Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

            var data = new RouteData(context.RouteData);
            if (parts.Length == 0)
            {
                data.Values["controller"] = "Container";
                data.Values["container"] = String.Empty;
            }
            else if (parts.Length == 1)
            {
                data.Values["controller"] = "Container";
                data.Values["container"] = parts[0];
            }
            else
            {
                data.Values["controller"] = "Blob";
                data.Values["container"] = parts[0];
                data.Values["blob"] = path.ToString().Replace(parts[0] + "/", "");
            }
            data.Values["action"] = request.Method;

            var query = request.Query;
            foreach (var parameter in query)
            {
                data.Values.Add(parameter.Key, parameter.Value);
            }

            context.RouteData = data;
            try
            {
                await _defaultHandler.RouteAsync(context);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
