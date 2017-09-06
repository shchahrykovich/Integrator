using YamlDotNet.Serialization;

namespace Runner
{
    public abstract class Stub
    {
        [YamlIgnore]
        public string FolderPath { get; set; }

        [YamlIgnore]
        public string FilePath { get; set; }

        [YamlIgnore]
        public string Name { get; set; }

        public bool? Stop { get; set; }

        public VerificationInfo Verify { get; set; }

        public VerificationInfo GetVerificationInfo()
        {
            if (null == Verify)
            {
                return new VerificationInfo
                {
                    AtLeast = 1
                };
            }
            else
            {
                return Verify;
            }
        }

        protected Stub()
        {
        }
    }

    public class VerificationInfo
    {
        public int? AtLeast { get; set; }

        public int? Exactly { get; set; }
    }
}
