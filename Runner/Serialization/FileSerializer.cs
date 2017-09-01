using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;

namespace Runner.Serialization
{
    internal static class FileSerializer
    {
        private const string ConfigFileName = "config.json";
        private const string StubFilePattern = "*.yml";

        private static readonly JsonSerializer Serializer = JsonSerializer.CreateDefault();
        private static readonly Deserializer YamlDesirializer = new DeserializerBuilder().Build();

        public static IEnumerable<TStub> ReadStubs<TStub>(string endpointFolder) where TStub: Stub
        {
            var stringProperties = typeof(TStub).GetProperties().Where(p => p.PropertyType == typeof(String)).ToArray();
            foreach (var stubFile in Directory.GetFiles(endpointFolder, StubFilePattern, SearchOption.AllDirectories))
            {
                using (var stubContent = new StringReader(File.ReadAllText(stubFile)))
                {
                    var stub = YamlDesirializer.Deserialize<TStub>(stubContent);

                    foreach (var property in stringProperties)
                    {
                        var value = property.GetValue(stub) as String;
                        if (!String.IsNullOrWhiteSpace(value))
                        {
                            var newValue = value.TrimEnd().Replace("\n", "\r\n");
                            property.SetValue(stub, newValue);
                        }
                    }

                    stub.FilePath = stubFile;
                    stub.FolderPath = endpointFolder;

                    yield return stub;
                }
            }
        }

        public static TConfig ReadConfig<TConfig>(string testFolder, TConfig defaultConfig)
        {
            TConfig result = defaultConfig;

            var configFile = Path.Combine(testFolder, ConfigFileName);
            if (File.Exists(configFile))
            {
                using (var config = new StringReader(File.ReadAllText(configFile)))
                {
                    using (var reader = new JsonTextReader(config))
                    {
                        result = Serializer.Deserialize<TConfig>(reader);
                    }
                }
            }

            return result;
        }

    }
}
