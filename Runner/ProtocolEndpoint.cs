﻿namespace Runner
{
    public abstract class ProtocolEndpoint
    {
        public abstract void Start();

        public abstract void Stop();

        public abstract void PrintSettings();

        public void Wait()
        {
            
        }
    }
}