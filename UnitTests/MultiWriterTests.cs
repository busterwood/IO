﻿using BusterWood.InputOutput;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture]
    public class MultiWriterTests
    {
        [Test]
        public void can_write_to_zero_writers()
        {
            var writer = Io.MultiWriter();
            var data = new Block<byte>(new byte[] { 1 });
            var res = writer.Write(data);
            Assert.AreEqual(data.Length, res.Bytes);
            Assert.IsNull(res.Error);
        }

        [Test]
        public void can_write_to_one_writers()
        {
            var mem = new MemoryWriter(new Block<byte>(0, 10));
            var writer = Io.MultiWriter(mem);
            var data = new Block<byte>(new byte[] { 1 });
            var res = writer.Write(data);
            Assert.AreEqual(data.Length, res.Bytes);
            Assert.IsNull(res.Error);
            Assert.AreEqual(1, mem.Data.Length, "Data.Length");
            Assert.AreEqual(1, mem.Data[0], "data[0]");
        }

        [Test]
        public void short_write_error_returned_if_only_part_of_input_is_written()
        {
            var mem = new StubWriter { Limit = 1 };
            var writer = Io.MultiWriter(mem);
            var data = new Block<byte>(new byte[] { 1, 2 });
            var res = writer.Write(data);
            Assert.AreEqual(1, res.Bytes);
            Assert.AreEqual(Io.ShortWrite, res.Error);
        }

        [Test]
        public void can_write_to_multiple_writers()
        {
            var mem1 = new MemoryWriter(new Block<byte>(0, 10));
            var mem2 = new MemoryWriter(new Block<byte>(0, 10));
            var writer = Io.MultiWriter(mem1, mem2);
            var data = new Block<byte>(new byte[] { 1 });
            var res = writer.Write(data);
            Assert.AreEqual(data.Length, res.Bytes);
            Assert.IsNull(res.Error);

            Assert.AreEqual(1, mem1.Data.Length, "Data.Length");
            Assert.AreEqual(1, mem1.Data[0], "data[0]");

            Assert.AreEqual(1, mem2.Data.Length, "Data.Length");
            Assert.AreEqual(1, mem2.Data[0], "data[0]");
        }
    }

    [TestFixture]
    public class MultiWriterAsyncTests
    {
        [Test]
        public async Task can_write_to_zero_writers()
        {
            var writer = Io.MultiWriter();
            var data = new Block<byte>(new byte[] { 1 });
            var res = await writer.WriteAsync(data);
            Assert.AreEqual(data.Length, res.Bytes);
            Assert.IsNull(res.Error);
        }

        [Test]
        public async Task can_write_to_one_writers()
        {
            var mem = new MemoryWriter(new Block<byte>(0, 10));
            var writer = Io.MultiWriter(mem);
            var data = new Block<byte>(new byte[] { 1 });
            var res = await writer.WriteAsync(data);
            Assert.AreEqual(data.Length, res.Bytes);
            Assert.IsNull(res.Error);
            Assert.AreEqual(1, mem.Data.Length, "Data.Length");
            Assert.AreEqual(1, mem.Data[0], "data[0]");
        }

        [Test]
        public async Task short_write_error_returned_if_only_part_of_input_is_written()
        {
            var mem = new StubWriter { Limit = 1 };
            var writer = Io.MultiWriter(mem);
            var data = new Block<byte>(new byte[] { 1, 2 });
            var res = await writer.WriteAsync(data);
            Assert.AreEqual(1, res.Bytes);
            Assert.AreEqual(Io.ShortWrite, res.Error);
        }

        [Test]
        public async Task can_write_to_multiple_writers()
        {
            var mem1 = new MemoryWriter(new Block<byte>(0, 10));
            var mem2 = new MemoryWriter(new Block<byte>(0, 10));
            var writer = Io.MultiWriter(mem1, mem2);
            var data = new Block<byte>(new byte[] { 1 });
            var res = await writer.WriteAsync(data);
            Assert.AreEqual(data.Length, res.Bytes);
            Assert.IsNull(res.Error);

            Assert.AreEqual(1, mem1.Data.Length, "Data.Length");
            Assert.AreEqual(1, mem1.Data[0], "data[0]");

            Assert.AreEqual(1, mem2.Data.Length, "Data.Length");
            Assert.AreEqual(1, mem2.Data[0], "data[0]");
        }
    }
}
