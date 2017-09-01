using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Runner
{
    public abstract class ProtocolEndpointSettings
    {
        [JsonIgnore]
        public String Name { get; set; }

        [JsonIgnore]
        public String FolderPath { get; set; }
    }
}
