using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Runner
{
    public abstract class ProtocolEndpoint<TSettings>: IProtocolEndpoint<TSettings>
        where TSettings: ProtocolEndpointSettings
    {
        public String Name => Settings.Name;

        public String FolderPath => Settings.FolderPath;

        public TSettings Settings { get; private set; }

        public CancellationToken Token { get; private set; }

        protected ProtocolEndpoint(TSettings settings)
        {
            Token = CancellationToken.None;
            Settings = settings;
        }

        public abstract void Stop();

        public Task StartAsync(CancellationToken token)
        {
            Token = token;
            Thread.MemoryBarrier();
            return StartInternalAsync();
        }

        public abstract Task StartInternalAsync();

        public abstract void PrintSettings(TextWriter log);

        public abstract TestExecutionStats GetStats();
    }

    public interface IProtocolEndpoint<out TSettings> :IProtocolEndpoint where TSettings : ProtocolEndpointSettings
    {
        TSettings Settings { get; }

        Task StartAsync(CancellationToken token);

        void PrintSettings(TextWriter log);

        TestExecutionStats GetStats();

        void Stop();
    }

    public interface IProtocolEndpoint
    {
        String Name { get; }
        string FolderPath { get; }
    }
}