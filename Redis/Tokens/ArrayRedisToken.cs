using System.Collections.Generic;

namespace Redis.Tokens
{
    public class ArrayRedisToken : RedisToken
    {
        private readonly int _length;
        private readonly List<RedisToken> _items;

        public IEnumerable<RedisToken> Items => _items;

        public ArrayRedisToken(int length)
        {
            _length = length;
            _items = new List<RedisToken>(_length);
        }

        public void Add(RedisToken token)
        {
            _items.Add(token);
        }

        public override object GetData()
        {
            return _items;
        }
    }
}
