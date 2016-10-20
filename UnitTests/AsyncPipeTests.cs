using BusterWood.InputOutput;
using NUnit.Framework;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture]
    public class AsyncPipeTests
    {
        [Test]
        public void test_single_read_and_async_writer_pair()
        {
            var pipe = Io.Pipe();
            var buf = new Block<byte>(new byte[64]);
            var testData = Encoding.UTF8.GetBytes("hello, world");
            var finished = new ManualResetEventSlim(false);
            Task.Run(() => AsyncCheckWrite(pipe.Writer, testData, finished));
            var res = pipe.Reader.Read(buf);
            Assert.IsNull(res.Error);
            Assert.AreEqual(12, res.Bytes);
            var got = Encoding.UTF8.GetString(buf.Array, 0, 12);
            Assert.AreEqual("hello, world", got);
            finished.Wait();
            pipe.Reader.Close();
            pipe.Writer.Close();
        }

        async Task AsyncCheckWrite(IWriteCloser w, byte[] data, ManualResetEventSlim finished)
        {
            var res = await w.WriteAsync(data);
            Assert.IsNull(res.Error);
            Assert.AreEqual(data.Length, res.Bytes);
            finished.Set();
        }

        [Test]
        public async Task test_single_async_read_and_async_writer_pair()
        {
            var pipe = Io.Pipe();
            var buf = new Block<byte>(new byte[64]);
            var testData = Encoding.UTF8.GetBytes("hello, world");
            var finished = new ManualResetEventSlim(false);
            Task.Run(() => AsyncCheckWrite(pipe.Writer, testData, finished));
            var res = await pipe.Reader.ReadAsync(buf);
            Assert.IsNull(res.Error);
            Assert.AreEqual(12, res.Bytes);
            var got = Encoding.UTF8.GetString(buf.Array, 0, 12);
            Assert.AreEqual("hello, world", got);
            finished.Wait();
            pipe.Reader.Close();
            pipe.Writer.Close();
        }

        [Test]
        public async Task test_single_async_read_and_sync_writer_pair()
        {
            var pipe = Io.Pipe();
            var buf = new Block<byte>(new byte[64]);
            var testData = Encoding.UTF8.GetBytes("hello, world");
            var finished = new ManualResetEventSlim(false);
            Task.Run(() => SyncCheckWrite(pipe.Writer, testData, finished));
            var res = await pipe.Reader.ReadAsync(buf);
            Assert.IsNull(res.Error);
            Assert.AreEqual(12, res.Bytes);
            var got = Encoding.UTF8.GetString(buf.Array, 0, 12);
            Assert.AreEqual("hello, world", got);
            finished.Wait();
            pipe.Reader.Close();
            pipe.Writer.Close();
        }

        void SyncCheckWrite(IWriteCloser w, byte[] data, ManualResetEventSlim finished)
        {
            var res = w.Write(data);
            Assert.IsNull(res.Error);
            Assert.AreEqual(data.Length, res.Bytes);
            finished.Set();
        }

        [Test]
        public void test_sequence_of_reads_and_writes()
        {
            var c = new BlockingCollection<int>();
            var pipe = Io.Pipe();
            ThreadPool.QueueUserWorkItem(_ => SyncReader(pipe.Reader, c));
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

        void SyncReader(IReader r, BlockingCollection<int> c)
        {
            Block<byte> buf = new byte[64];
            for (;;)
            {
                var res = r.Read(buf);
                if (res.Error == Io.EOF)
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

        [Test]
        public void test_sequence_of_async_reads_and_sync_writes()
        {
            var c = new BlockingCollection<int>();
            var pipe = Io.Pipe();
            Task.Run(() => AsyncReader(pipe.Reader, c));
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

        [Test]
        public async Task test_sequence_of_async_reads_and_async_writes()
        {
            var c = new BlockingCollection<int>();
            var pipe = Io.Pipe();
            Task.Run(() => AsyncReader(pipe.Reader, c));
            Block<byte> buf = new byte[64];
            for (var i = 0; i < 5; i++)
            {
                var p = buf.Slice(0, 5 + i * 10);
                var res = await pipe.Writer.WriteAsync(p);

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

        async Task AsyncReader(IReader r, BlockingCollection<int> c)
        {
            Block<byte> buf = new byte[64];
            for (;;)
            {
                var res = await r.ReadAsync(buf);
                if (res.Error == Io.EOF)
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

        [Test]
        public async Task test_sequence_of_sync_reads_and_async_writes()
        {
            var c = new BlockingCollection<int>();
            var pipe = Io.Pipe();
            Task.Run(() => SyncReader(pipe.Reader, c));
            Block<byte> buf = new byte[64];
            for (var i = 0; i < 5; i++)
            {
                var p = buf.Slice(0, 5 + i * 10);
                var res = await pipe.Writer.WriteAsync(p);

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
    }
}
