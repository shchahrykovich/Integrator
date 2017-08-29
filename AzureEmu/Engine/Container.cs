using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace AzureEmu.Engine
{
    public class Container
    {
        private readonly ConcurrentDictionary<String, Blob> _storage;

        public String Name { get; set; }

        public Container()
        {
            _storage = new ConcurrentDictionary<string, Blob>(StringComparer.OrdinalIgnoreCase);
        }

        public Container(Blob blob) : this()
        {
            _storage.TryAdd(blob.Path, blob);
        }

        public Blob Get(string blobName)
        {
            if (_storage.TryGetValue(blobName, out Blob b))
            {
                return b;
            }
            throw new KeyNotFoundException(blobName);
        }

        public bool Contains(string blobName)
        {
            return _storage.ContainsKey(blobName);
        }

        public void Update(Blob blob)
        {
            _storage.AddOrUpdate(blob.Path, blob, (name, b) => b);
        }
    }
}
