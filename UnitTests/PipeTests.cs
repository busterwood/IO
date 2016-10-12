using BusterWood.InputOutput;
using NUnit.Framework;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace UnitTests
{
    [TestFixture]
    public class PipeTests
    {
        [Test]
        public void test_single_read_writer_pair()
        {
            var pipe = IO.Pipe();
            var buf = new Block<byte>(new byte[64]);
            var testData = Encoding.UTF8.GetBytes("hello, world");
            var finished = new ManualResetEventSlim(false);
            ThreadPool.QueueUserWorkItem(_ => CheckWrite(pipe.Writer, testData, finished));
            var res = pipe.Reader.Read(buf);
            Assert.IsNull(res.Error);
            Assert.AreEqual(12, res.Bytes);
            var got = Encoding.UTF8.GetString(buf.Array, 0, 12);
            Assert.AreEqual("hello, world", got);
            finished.Wait();
            pipe.Reader.Close();
            pipe.Writer.Close();
        }

        void CheckWrite(IWriter w, byte[] data, ManualResetEventSlim finished)
        {
            var res = w.Write(data);
            Assert.IsNull(res.Error);
            Assert.AreEqual(data.Length, res.Bytes);
            finished.Set();
        }

        [Test]
        public void test_sequence_of_reader_and_writes()
        {
            var c = new BlockingCollection<int>();
            var pipe = IO.Pipe();
            ThreadPool.QueueUserWorkItem(_ => Reader(pipe.Reader, c));
            Block<byte> buf = new byte[64];
            for (var i = 0; i < 5; i++)
            {
                var p = buf.Slice(0, 5 + i * 10);
                var res = pipe.Writer.Write(p);

                Assert.AreEqual(p.Length, res.Bytes, "did not write all bytes");
                Assert.IsNull(res.Error);

                var nn = c.Take();
                Assert.AreEqual(res.Bytes, nn, "wrote and read differ");
            }
            pipe.Writer.Close();
            var nLast = c.Take();
            Assert.AreEqual(0, nLast, "last");
            pipe.Reader.Close();
        }

        void Reader(IReader r, BlockingCollection<int> c)
        {
            Block<byte> buf = new byte[64];
            for (;;)
            {
                var res = r.Read(buf);
                if (res.Error == IO.EOF)
                {
                    c.Add(0);
                    break;
                }
                if (res.Error != null)
                {
                    Assert.Fail("read: %v", res.Error);
                }
                c.Add(res.Bytes);
            }
        }
    }
}