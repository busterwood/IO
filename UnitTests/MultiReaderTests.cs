using BusterWood.InputOutput;
using NUnit.Framework;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture]
    public class MultiReaderTests
    {
        [Test]
        public void can_read_from_zero_readers()
        {
            var reader = IO.MultiReader();
            var dest = new Block<byte>(10);
            var res = reader.Read(dest);
            Assert.AreEqual(0, res.Bytes, "bytesRead");
            Assert.AreEqual(IO.EOF, res.Error, "Error");
        }

        [Test]
        public void can_read_from_one_readers()
        {
            var data = new byte[] { 1, 2 };
            var reader = IO.MultiReader(IO.Reader(data));
            var dest = new Block<byte>(10);
            var res = reader.Read(dest);
            Assert.AreEqual(2, res.Bytes, "bytesRead");
            Assert.AreEqual(null, res.Error, "Error");
            Assert.AreEqual(1, dest[0]);
            Assert.AreEqual(2, dest[1]);
        }

        [Test]
        public void reading_past_end_of_data_returns_EOF()
        {
            var data = new byte[] { 1, 2 };
            var reader = IO.MultiReader(IO.Reader(data));
            var dest = new Block<byte>(10);
            var res = reader.Read(dest);
            res = reader.Read(dest);
            Assert.AreEqual(0, res.Bytes, "bytesRead");
            Assert.AreEqual(IO.EOF, res.Error, "Error");
        }

        [Test]
        public void can_read_from_partial_data_from_mutliple_readers()
        {
            var reader = IO.MultiReader(IO.Reader(new byte[] { 1, 2, 3 }), IO.Reader(new byte[] { 4, 5, 6 }));
            var dest = new Block<byte>(2);
            var res = reader.Read(dest);
            Assert.AreEqual(2, res.Bytes, "bytesRead");
            Assert.AreEqual(null, res.Error, "Error");
            Assert.AreEqual(1, dest[0]);
            Assert.AreEqual(2, dest[1]);

            res = reader.Read(dest);
            Assert.AreEqual(1, res.Bytes, "bytesRead");
            Assert.AreEqual(null, res.Error, "Error");
            Assert.AreEqual(3, dest[0]);

            res = reader.Read(dest);
            Assert.AreEqual(2, res.Bytes, "bytesRead");
            Assert.AreEqual(null, res.Error, "Error");
            Assert.AreEqual(4, dest[0]);
            Assert.AreEqual(5, dest[1]);

            res = reader.Read(dest);
            Assert.AreEqual(1, res.Bytes, "bytesRead");
            Assert.AreEqual(null, res.Error, "Error");
            Assert.AreEqual(6, dest[0]);

            res = reader.Read(dest);
            Assert.AreEqual(0, res.Bytes, "bytesRead");
            Assert.AreEqual(IO.EOF, res.Error, "Error");
        }

    }

    [TestFixture]
    public class MultiReaderAsyncTests
    {
        [Test]
        public async Task can_read_from_zero_readers()
        {
            var reader = IO.MultiReader();
            var dest = new Block<byte>(10);
            var res = await reader.ReadAsync(dest);
            Assert.AreEqual(0, res.Bytes, "bytesRead");
            Assert.AreEqual(IO.EOF, res.Error, "Error");
        }

        [Test]
        public async Task can_read_from_one_readers()
        {
            var data = new byte[] { 1, 2 };
            var reader = IO.MultiReader(IO.Reader(data));
            var dest = new Block<byte>(10);
            var res = await reader.ReadAsync(dest);
            Assert.AreEqual(2, res.Bytes, "bytesRead");
            Assert.AreEqual(null, res.Error, "Error");
            Assert.AreEqual(1, dest[0]);
            Assert.AreEqual(2, dest[1]);
        }

        [Test]
        public async Task reading_past_end_of_data_returns_EOF()
        {
            var data = new byte[] { 1, 2 };
            var reader = IO.MultiReader(IO.Reader(data));
            var dest = new Block<byte>(10);
            var res = await reader.ReadAsync(dest);
            res = await reader.ReadAsync(dest);
            Assert.AreEqual(0, res.Bytes, "bytesRead");
            Assert.AreEqual(IO.EOF, res.Error, "Error");
        }

        [Test]
        public async Task can_read_from_data_from_mutliple_readers()
        {
            var reader = IO.MultiReader(IO.Reader(new byte[] { 1, 2, 3 }), IO.Reader(new byte[] { 4, 5, 6 }));
            var dest = new Block<byte>(2);
            var res = await reader.ReadAsync(dest);
            Assert.AreEqual(2, res.Bytes, "bytesRead");
            Assert.AreEqual(null, res.Error, "Error");
            Assert.AreEqual(1, dest[0]);
            Assert.AreEqual(2, dest[1]);

            res = await reader.ReadAsync(dest);
            Assert.AreEqual(1, res.Bytes, "bytesRead");
            Assert.AreEqual(null, res.Error, "Error");
            Assert.AreEqual(3, dest[0]);

            res = await reader.ReadAsync(dest);
            Assert.AreEqual(2, res.Bytes, "bytesRead");
            Assert.AreEqual(null, res.Error, "Error");
            Assert.AreEqual(4, dest[0]);
            Assert.AreEqual(5, dest[1]);

            res = await reader.ReadAsync(dest);
            Assert.AreEqual(1, res.Bytes, "bytesRead");
            Assert.AreEqual(null, res.Error, "Error");
            Assert.AreEqual(6, dest[0]);

            res = await reader.ReadAsync(dest);
            Assert.AreEqual(0, res.Bytes, "bytesRead");
            Assert.AreEqual(IO.EOF, res.Error, "Error");
        }
    }
}
