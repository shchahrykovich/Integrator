﻿using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Runner
{
    public class Test
    {
        public String Cmd { get; set; }

        public String[] Args { get; set; }

        [JsonIgnore]
        public string Name { get; set; }

        [JsonIgnore]
        public string Folder { get; set; }

        [JsonIgnore]
        public IEnumerable<ProtocolEndpoint> Endpoints => _endpoints;

        private readonly List<ProtocolEndpoint> _endpoints = new List<ProtocolEndpoint>();

        public void AddEndpoint(ProtocolEndpoint endpoint)
        {
            _endpoints.Add(endpoint);
        }
    }
}