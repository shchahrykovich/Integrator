using System;

namespace AzureEmu.Engine
{
    public class Blob
    {
        public Blob(string blobName, byte[] content)
        {
            Path = blobName;
            Bytes = content;
        }

        public String Path { get; set; }
        public byte[] Bytes { get; set; }
    }
}