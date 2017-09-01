using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Runner.AzureBlobService
{
    public class BlobFileStub : Stub
    {
        public String Content { get; set; }

        public BlobFormat Format { get; set; }

        public string GetContainerName()
        {
            var relativePath = FilePath.Substring(FolderPath.Length + 1);
            var containerName = relativePath
                .Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).First();
            return containerName;
        }

        public string GetBlobName()
        {
            var container = GetContainerName();
            var blobName = FilePath.Substring(FolderPath.Length + container.Length + 1)
                .Replace(Path.DirectorySeparatorChar, '/');
            blobName = blobName.Replace(".yml", "");
            return blobName;
        }

        public byte[] GetBytes()
        {
            byte[] conten = Encoding.UTF8.GetBytes(Content);
            switch (Format)
            {
                case BlobFormat.Utf8:
                    break;
                case BlobFormat.Deflate:
                {
                    using (var output = new MemoryStream())
                    {
                        using (var deflate = new DeflateStream(output, CompressionMode.Compress))
                        {
                            deflate.Write(conten, 0, conten.Length);
                        }

                        conten = output.ToArray();
                    }
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(Format.ToString());
            }
            return conten;
        }
    }
}
