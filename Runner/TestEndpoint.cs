namespace Runner
{
    internal abstract class TestEndpoint
    {
        public abstract void Start();

        public abstract void Stop();

        public abstract void PrintSettings();

        public void Wait()
        {
            
        }
    }
}