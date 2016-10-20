using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO;
using BusterWood.InputOutput;

namespace UnitTests
{
    [TestFixture]
    public class StreamReaderWrapperTests
    {
        [Test]
        public void can_read_from_a_stream()
        {
            var input = new MemoryStream(new byte[] { 1, 2, 3, 4 });
            IReader reader = Io.Reader(input);
            var buf = new byte[4];
            var res = reader.Read(buf);
            Assert.AreEqual(4, res.Bytes, "Bytes read");
            Assert.IsNull(res.Error, "Error");
            Assert.AreEqual(1, buf[0], "buf[0]");
            Assert.AreEqual(2, buf[1], "buf[1]");
            Assert.AreEqual(3, buf[2], "buf[2]");
            Assert.AreEqual(4, buf[3], "buf[3]");
        }

        [Test]
        public void read_returns_EOF_when_stream_empty()
        {
            var input = new MemoryStream(new byte[] {});
            IReader reader = Io.Reader(input);
            var buf = new byte[4];
            var res = reader.Read(buf);
            Assert.AreEqual(0, res.Bytes, "Bytes read");
            Assert.AreEqual(Io.EOF, res.Error, "Error");
        }

        [Test]
        public void read_returns_EOF_after_all_data_read_from_stream()
        {
            var input = new MemoryStream(new byte[] {1, 2});
            IReader reader = Io.Reader(input);
            var buf = new byte[2];
            var res = reader.Read(buf);
            Assert.AreEqual(2, res.Bytes, "Bytes read");
            Assert.IsNull(res.Error, "Error");
            Assert.AreEqual(1, buf[0], "buf[0]");
            Assert.AreEqual(2, buf[1], "buf[1]");

            res = reader.Read(buf);
            Assert.AreEqual(0, res.Bytes, "Bytes read");
            Assert.AreEqual(Io.EOF, res.Error, "Error");
        }

        [Test]
        public void object_disposed_exception_is_returned_in_the_result()
        {
            var input = new MemoryStream(new byte[] {1, 2});
            IReader reader = Io.Reader(input);
            input.Dispose();
            var buf = new byte[2];
            var res = reader.Read(buf);
            Assert.AreEqual(0, res.Bytes, "Bytes read");
            Assert.IsInstanceOf<ObjectDisposedException>(res.Error, "Error");
        }

    }
}
