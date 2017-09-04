using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Runner.Serialization
{
    internal static class FileSerializer
    {
        private const string ConfigFileName = "config.json";
        private const string StubFilePattern = "*.yml";

        private static readonly JsonSerializer Serializer = JsonSerializer.CreateDefault();
        private static readonly Deserializer YamlDesirializer = new DeserializerBuilder().Build();
        private static readonly Serializer YamlSerializer = new SerializerBuilder().Build();

        public static IEnumerable<TStub> ReadStubs<TStub>(string endpointFolder) where TStub : Stub
        {
            var stringProperties = typeof(TStub).GetProperties().Where(p => p.PropertyType == typeof(String)).ToArray();
            foreach (var stubFile in Directory.GetFiles(endpointFolder, StubFilePattern, SearchOption.AllDirectories))
            {
                if (stubFile.Contains(@"\_Missing\"))
                {
                    continue;
                }
                using (var fileContent = new StringReader(File.ReadAllText(stubFile)))
                {
                    var parser = new Parser(fileContent);
                    parser.Expect<StreamStart>();

                    while (parser.Accept<DocumentStart>())
                    {
                        var stub = YamlDesirializer.Deserialize<TStub>(parser);

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
        }

        public static TConfig ReadConfig<TConfig>(string testFolder, TConfig defaultConfig)
            where TConfig : ProtocolEndpointSettings
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

            result.Name = Path.GetFileName(testFolder);
            result.FolderPath = testFolder;
            return result;
        }

        public static void WriteStub(string path, Stub stub)
        {
            var content = YamlSerializer.Serialize(stub);
            if (File.Exists(path))
            {
                File.WriteAllText(path, content);
            }
            else
            {
                DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(path));
                if (dir.Exists)
                {
                    File.WriteAllText(path, content);
                }
                else
                {
                    dir.Create();
                    File.WriteAllText(path, content);
                }
            }
        }
    }
}
