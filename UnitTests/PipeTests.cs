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
            var pipe = Io.Pipe();
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

        void CheckWrite(IWriteCloser w, byte[] data, ManualResetEventSlim finished)
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
        public void test_a_large_write_that_requires_multiple_reads_to_satisfy()
        {
            var c = new BlockingCollection<IOResult>();
            var pipe = Io.Pipe();
            Block<byte> wdat = new byte[128];
            for (var i = 0; i < wdat.Length; i++) {
                wdat[i] = (byte)i;
            }
            ThreadPool.QueueUserWorkItem(_ => Writer(pipe.Writer, wdat, c));
            Block<byte> rdat = new byte[1024];
            var tot = 0;
            for (var n = 1; n <= 256; n*=2)
            {
                var res = pipe.Reader.Read(rdat.Slice(tot, tot + n));
                if (res.Error != null & res.Error != Io.EOF)
                    Assert.Fail("Error reading: " + res.Error);

                // only final two reads should be short - 1 byte, then 0
                var expect = n;
    			if (n == 128) {
                    expect = 1;
    			} else if (n == 256) {
                    expect = 0;
    				if (res.Error != Io.EOF) {
                        Assert.Fail("read at end: " + res.Error);
    				}
    			}
                Assert.AreEqual(expect, res.Bytes, "read did not match expected");
                tot += res.Bytes;
            }
            var pr = c.Take();
            if (pr.Bytes != 128 || pr.Error != null) {
                Assert.Fail($"write 128: {pr.Bytes}, {pr.Error}");
    		}
            Assert.AreEqual(128, tot, "total read");
    		for (byte i = 0; i < 128; i++) {
    			if (rdat[i] != i) {
                    Assert.Fail($"rdat[{i}] = {rdat[i]}");
                }
    		}
        }

        void Writer(IWriteCloser w, Block<byte> buf, BlockingCollection<IOResult> c)
        {
            var res = w.Write(buf);
            w.Close();
            c.Add(res);
        }
    }
}