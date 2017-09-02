using System.IO;
using System.Threading;

namespace Runner
{
    public abstract class ProtocolEndpoint<TSettings>: IProtocolEndpoint<TSettings>
        where TSettings: ProtocolEndpointSettings
    {
        public TSettings Settings { get; private set; }

        public CancellationToken Token { get; private set; }

        protected ProtocolEndpoint(CancellationToken token, TSettings settings)
        {
            Token = token;
            Settings = settings;
        }

        public abstract void Start();

        public abstract void Stop();

        public abstract void PrintSettings(TextWriter log);

        public abstract TestExecutionStats GetStats();
    }

    public interface IProtocolEndpoint<out TSettings> where TSettings : ProtocolEndpointSettings
    {
        TSettings Settings { get; }

        void Start();

        void Stop();

        void PrintSettings(TextWriter log);

        TestExecutionStats GetStats();
    }
}