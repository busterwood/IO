using BusterWood.InputOutput;
using NUnit.Framework;
using System.Threading.Tasks;
using System;

namespace UnitTests
{
    [TestFixture]
    public class TeeTests
    {
        [Test]
        public void reading_writes_the_data_read()
        {
            var output = new MemoryWriter(new Block<byte>(0, 10));
            var input = new MemoryReader(new byte[] { 1, 2 });
            IReader tee = IO.Tee(input, output);
            var buf = new Block<byte>(10);

            // can read only
            var res = tee.Read(buf);
            Assert.AreEqual(2, res.Bytes, "Bytes");
            Assert.AreEqual(null, res.Error, "Error");

            // data copied to writer
            Assert.AreEqual(2, output.Data.Length, "Data.Length");
            Assert.AreEqual(1, output.Data[0], "Data[0]");
            Assert.AreEqual(2, output.Data[1], "Data[1]");
        }

        [Test]
        public void write_errors_are_returned_as_read_errors()
        {
            var output = new StubWriter { Error = new Exception() };
            var input = new MemoryReader(new byte[] { 1, 2 });
            IReader tee = IO.Tee(input, output);
            var buf = new Block<byte>(10);
            var res = tee.Read(buf);
            Assert.AreEqual(0, res.Bytes, "Bytes");
            Assert.AreEqual(output.Error, res.Error, "Error");
        }
    }

    class StubWriter : IWriter
    {
        public Exception Error;
        public IOResult Write(Block<byte> buf) => new IOResult(0, Error);

        public Task<IOResult> WriteAsync(Block<byte> buf) => Task.FromResult(new IOResult(0, Error));
    }

    [TestFixture]
    public class TeeAsyncTests
    {
        [Test]
        public async Task reading_writes_the_data_read()
        {
            var output = new MemoryWriter(new Block<byte>(0, 10));
            var input = new MemoryReader(new byte[] { 1, 2 });
            IReader tee = IO.Tee(input, output);
            var buf = new Block<byte>(10);

            // can read only
            var res = await tee.ReadAsync(buf);
            Assert.AreEqual(2, res.Bytes, "Bytes");
            Assert.AreEqual(null, res.Error, "Error");

            // data copied to writer
            Assert.AreEqual(2, output.Data.Length, "Data.Length");
            Assert.AreEqual(1, output.Data[0], "Data[0]");
            Assert.AreEqual(2, output.Data[1], "Data[1]");
        }

        [Test]
        public async Task write_errors_are_returned_as_read_errors()
        {
            var output = new StubWriter { Error = new Exception() };
            var input = new MemoryReader(new byte[] { 1, 2 });
            IReader tee = IO.Tee(input, output);
            var buf = new Block<byte>(10);
            var res = await tee.ReadAsync(buf);
            Assert.AreEqual(0, res.Bytes, "Bytes");
            Assert.AreEqual(output.Error, res.Error, "Error");
        }
    }
}
