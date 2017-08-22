using Amqp.Listener;
using System;

namespace Queue
{
    public class Class1
    {
        public void Test()
        {
            Uri uri = new Uri("amqp://127.0.0.1:4545");
            ContainerHost host = new ContainerHost(uri);
            host.Open();

            host.RegisterMessageProcessor(uri.AbsolutePath, new MessageProcessor());

            host.Close();
        }
    }
}
