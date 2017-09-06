using System.Linq;
using System.Text;
using System.Threading;
using Xunit;

namespace Redis.Tests
{
    public class RedisStreamReaderTests
    {
        [Fact]
        public void ReadBytes_Should_Return_Array()
        {
            // Arrange
            using (var stream = new FakeStream("AAAAAAAAAABBBBBBBBBBCCCCCCCCCC"))
            {
                var reader = new RedisStreamReader(stream, CancellationToken.None, 10);

                byte[] result = reader.ReadBytes(5);
                AssertContent(result, "AAAAA");

                result = reader.ReadBytes(1);
                AssertContent(result, "A");

                result = reader.ReadBytes(5);
                AssertContent(result, "AAAAB");
            }
        }

        private static void AssertContent(byte[] result, string expected)
        {
            Assert.Equal(Encoding.UTF8.GetString(result), expected);
        }
    }
}