using YamlDotNet.Serialization;

namespace Runner
{
    public abstract class Stub
    {
        [YamlIgnore]
        public string FolderPath { get; set; }

        [YamlIgnore]
        public string FilePath { get; set; }
    }
}
