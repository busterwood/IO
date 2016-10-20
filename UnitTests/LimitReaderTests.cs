using BusterWood.InputOutput;
using NUnit.Framework;
using System.Threading.Tasks;

namespace UnitTests
{

    [TestFixture]
    public class LimitReaderTests
    {
        [Test]
        public void eof_returned_at_end_of_data()
        {
            var limited = new byte[] { }.Reader().Limit(10);
            var dest = new Block<byte>(new byte[10]);
            IOResult res = limited.Read(dest);
            Assert.AreEqual(0, res.Bytes, "bytesRead");
            Assert.AreEqual(Io.EOF, res.Error, "Error");
        }

        [Test]
        public void can_read_less_than_limit()
        {
            var limited = new byte[] { 1 }.Reader().Limit(10);
            var dest = new Block<byte>(new byte[10]);
            IOResult res = limited.Read(dest);
            Assert.AreEqual(1, res.Bytes, "bytesRead");
            Assert.IsNull(res.Error, "Error");
            Assert.AreEqual(1, dest[0], "dest[0]");
        }

        [Test]
        public void can_read_up_to_limit()
        {
            var limited = new byte[] { 1, 2, 3, 4, 5 }.Reader().Limit(5);
            var dest = new Block<byte>(new byte[5]);
            IOResult res = limited.Read(dest);
            Assert.AreEqual(5, res.Bytes, "bytesRead");
            Assert.IsNull(res.Error, "Error");
            for(int i = 1; i <= 5; i++)
                Assert.AreEqual(i, dest[i-1], "dest[i]");
        }

        [Test]
        public void eof_returned_after_limit_reached()
        {
            var limited = new byte[] { 1,2,3,4,5 }.Reader().Limit(5);
            var dest = new Block<byte>(new byte[5]);
            IOResult res = limited.Read(dest);
            Assert.IsNull(res.Error, "Error");
            res = limited.Read(dest);
            Assert.AreEqual(0, res.Bytes, "bytesRead");
            Assert.AreEqual(Io.EOF, res.Error, "Error");
        }
    }

    [TestFixture]
    public class LimitReaderAsyncTests
    {
        [Test]
        public async Task eof_returned_at_end_of_data()
        {
            var limited = new byte[] { }.Reader().Limit(10);
            var dest = new Block<byte>(new byte[10]);
            IOResult res = await limited.ReadAsync(dest);
            Assert.AreEqual(0, res.Bytes, "bytesRead");
            Assert.AreEqual(Io.EOF, res.Error, "Error");
        }

        [Test]
        public async Task can_read_less_than_limit()
        {
            var limited = new byte[] { 1 }.Reader().Limit(10);
            var dest = new Block<byte>(new byte[10]);
            IOResult res = await limited.ReadAsync(dest);
            Assert.AreEqual(1, res.Bytes, "bytesRead");
            Assert.IsNull(res.Error, "Error");
            Assert.AreEqual(1, dest[0], "dest[0]");
        }

        [Test]
        public async Task can_read_up_to_limit()
        {
            var limited = new byte[] { 1, 2, 3, 4, 5 }.Reader().Limit(5);
            var dest = new Block<byte>(new byte[5]);
            IOResult res = await limited.ReadAsync(dest);
            Assert.AreEqual(5, res.Bytes, "bytesRead");
            Assert.IsNull(res.Error, "Error");
            for(int i = 1; i <= 5; i++)
                Assert.AreEqual(i, dest[i-1], "dest[i]");
        }

        [Test]
        public async Task eof_returned_after_limit_reached()
        {
            var limited = new byte[] { 1, 2, 3, 4, 5 }.Reader().Limit(5);
            var dest = new Block<byte>(new byte[5]);
            IOResult res = await limited.ReadAsync(dest);
            Assert.IsNull(res.Error, "Error");
            res = await limited.ReadAsync(dest);
            Assert.AreEqual(0, res.Bytes, "bytesRead");
            Assert.AreEqual(Io.EOF, res.Error, "Error");
        }
    }
}
