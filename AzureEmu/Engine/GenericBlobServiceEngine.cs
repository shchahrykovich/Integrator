using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AzureEmu.Engine
{
    public class GenericBlobServiceEngine : IBlobServiceEngine
    {
        private readonly ConcurrentDictionary<String, Container> _storage;

        public GenericBlobServiceEngine()
        {
            _storage = new ConcurrentDictionary<string, Container>(StringComparer.OrdinalIgnoreCase);
        }

        public byte[] GetBlob(string containerName, string blobName)
        {
            if (_storage.TryGetValue(containerName, out Container c))
            {
                var blob = c.Get(blobName);
                return blob.Bytes;
            }

            throw new KeyNotFoundException(containerName + "/" + blobName);
        }

        public bool ContainsBlob(string containerName, string blobName)
        {
            if (_storage.TryGetValue(containerName, out Container c))
            {
                return c.Contains(blobName);
            }

            return false;
        }

        public void PutBlob(string containerName, string blobName, byte[] content)
        {
            var blob = new Blob(blobName, content);
            _storage.AddOrUpdate(containerName, new Container(blob), (name, c) =>
            {
                c.Add(blob);
                return c;
            });
        }

        public bool ContainsContainer(string containerName)
        {
            return _storage.ContainsKey(containerName);
        }

        public Container CreateContainer(String name)
        {
            return _storage.AddOrUpdate(name, new Container {Name = name}, (n, c) => c);
        }

        public void Add(string containerName, string blobName, byte[] bytes)
        {
            var container = new Container
            {
                Name = containerName
            };
            if(!_storage.TryAdd(containerName, container))
            {
                container = _storage[containerName];
            }

            var blob = new Blob(blobName, bytes);
            container.Add(blob);
        }
    }
}
