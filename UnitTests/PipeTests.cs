using BusterWood.InputOutput;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture]
    public class PipeTests
    {
        [Test]
        public void test_single_read_writer_pair()
        {
            var ends = IO.Pipe();
            var buf = new Block<byte>(new byte[64]);
            var testData = Encoding.UTF8.GetBytes("hello, world");
            var finished = new ManualResetEventSlim(false);
            ThreadPool.QueueUserWorkItem(_ => CheckWrite(ends.Writer, testData, finished));
            var res = ends.Reader.Read(buf);
            Assert.IsNull(res.Error);
            Assert.AreEqual(12, res.Bytes);
            var got = Encoding.UTF8.GetString(buf.Array, 0, 12);
            Assert.AreEqual("hello, world", got);
            finished.Wait();
            ends.Reader.Close();
            ends.Writer.Close();
        }

        void CheckWrite(IWriter w, byte[] data, ManualResetEventSlim finished)
        {
            var res = w.Write(data);
            Assert.IsNull(res.Error);
            Assert.AreEqual(data.Length, res.Bytes);
            finished.Set();
        }
    }
}
