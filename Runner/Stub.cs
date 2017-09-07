using YamlDotNet.Serialization;

namespace Runner
{
    public abstract class Stub
    {
        [YamlIgnore]
        public string FolderPath { get; set; }

        [YamlIgnore]
        public int DocumentIndex { get; set; }

        [YamlIgnore]
        public string FilePath { get; set; }

        [YamlIgnore]
        public string Name { get; set; }

        public VerificationInfo Verify { get; set; }
    }

    public class VerificationInfo
    {
        public int? AtLeast { get; set; }

        public int? Exactly { get; set; }
    }
}
