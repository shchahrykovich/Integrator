using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Runner
{
    public class Test: ProtocolEndpointSettings
    {
        public String Cmd { get; set; }

        public String[] Args { get; set; }

        [JsonIgnore]
        public IEnumerable<IProtocolEndpoint<ProtocolEndpointSettings>> Endpoints => _endpoints;

        private readonly List<IProtocolEndpoint<ProtocolEndpointSettings>> _endpoints = new List<IProtocolEndpoint<ProtocolEndpointSettings>>();

        public void AddEndpoint(IProtocolEndpoint<ProtocolEndpointSettings> endpoint)
        {
            _endpoints.Add(endpoint);
        }
    }
}