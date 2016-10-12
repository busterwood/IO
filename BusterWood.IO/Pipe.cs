using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BusterWood.InputOutput
{
    class Pipe : IReader, IWriter
    {
        static readonly IOException PipeClosed = new IOException("Pipe has been closed");

        readonly SemaphoreSlim readLock = new SemaphoreSlim(1, 1); // only one reader at a time
        readonly SemaphoreSlim writeLock = new SemaphoreSlim(1, 1); // only one writer at a time
        readonly SemaphoreSlim gate = new SemaphoreSlim(1, 1);  // one one reader OR writer at a time
        readonly ConditionVariable readCV; // used as a condition variable to signal a waiting reader
        readonly ConditionVariable writeCV; // used as a condition variable to signal a waiting writer
        Block<byte> data;
        Exception readError;  // error to give to writes if the reader is closed 
        Exception writeError; // error to give to reads if the writer is closed 

        public Pipe()
        {
            readCV = new ConditionVariable(gate);
            writeCV = new ConditionVariable(gate);
        }

        public IOResult Read(Block<byte> buf)
        {
            using (readLock.Lock())
            using (gate.Lock())
            {
                for (;;)
                {
                    if (readError != null)
                        return new IOResult(0, PipeClosed);
                    if (data.Length > 0)
                        break;
                    if (writeError != null)
                        return new IOResult(0, writeError);

                    readCV.Wait();
                }
                int bytesCopied = data.CopyTo(buf);
                data = data.Slice(bytesCopied);
                if (data.Length == 0)
                {
                    data = new Block<byte>();
                    writeCV.Signal();
                }
                return new IOResult(bytesCopied, null);
            }
        }

        public Task<IOResult> ReadAsync(Block<byte> buf)
        {
            throw new NotImplementedException();
        }

        public IOResult Write(Block<byte> buf)
        {
            Exception err = null;
            using (writeLock.Lock())
            using (gate.Lock())
            {
                if (writeError != null)
                    return new IOResult(0, PipeClosed);
                data = buf;
                readCV.Signal();

                for (;;)
                {
                    if (data.Length == 0)
                        break;
                    if (readError != null)
                    {
                        err = readError;
                        break;
                    }
                    if (writeError != null)
                        err = PipeClosed;

                    writeCV.Wait();
                }
                var n = buf.Length - data.Length;
                data = new Block<byte>();
                return new IOResult(n, err);
            }
        }

        public Task<IOResult> WriteAsync(Block<byte> buf)
        {
            throw new NotImplementedException();
        }

        public void ReaderClose(Exception err)
        {
            using (gate.Lock())
            {
                readError = err ?? PipeClosed;
                readCV.Signal();
                writeCV.Signal();
            }
        }

        public void WriterClose(Exception err)
        {
            using (gate.Lock())
            {
                writeError = err ?? IO.EOF;
                readCV.Signal();
                writeCV.Signal();
            }
        }
    }

    public class PipeReader : IReadCloser
    {
        readonly Pipe pipe;

        internal PipeReader(Pipe pipe)
        {
            this.pipe = pipe;
        }

        public IOResult Read(Block<byte> buf) => pipe.Read(buf);

        public Task<IOResult> ReadAsync(Block<byte> buf) => pipe.ReadAsync(buf);

        public void Close(Exception err = null) => pipe.ReaderClose(err);

        public void Close() => pipe.ReaderClose(null);
    }

    public class PipeWriter : IWriteCloser
    {
        readonly Pipe pipe;

        internal PipeWriter(Pipe pipe)
        {
            this.pipe = pipe;
        }

        public IOResult Write(Block<byte> buf) => pipe.Write(buf);

        public Task<IOResult> WriteAsync(Block<byte> buf) => pipe.WriteAsync(buf);

        public void Close(Exception err = null) => pipe.WriterClose(err);

        public void Close() => pipe.WriterClose(null);
    }

    public static partial class Extensions
    {
        internal static SemaphoreReleaser Lock(this SemaphoreSlim gate)
        {
            gate.Wait();
            return new SemaphoreReleaser(gate);
        }

        internal static async Task<SemaphoreReleaser> LockAsync(this SemaphoreSlim gate)
        {
            await gate.WaitAsync();
            return new SemaphoreReleaser(gate);
        }
    }

    struct SemaphoreReleaser : IDisposable
    {
        readonly SemaphoreSlim gate;

        public SemaphoreReleaser(SemaphoreSlim gate)
        {
            this.gate = gate;
        }

        public void Dispose()
        {
            gate?.Release();
        }
    }

    class ConditionVariable
    {
        readonly SemaphoreSlim gate;
        readonly SemaphoreSlim cv;
        volatile bool waiting;

        public ConditionVariable(SemaphoreSlim gate)
        {
            this.gate = gate;
            cv = new SemaphoreSlim(0, 1);
            waiting = false;
        }

        /// <summary>Must only be called when holding the lock <see cref="gate"/></summary>
        public void Signal()
        {
            if (waiting)
                cv.Release();
        }

        /// <summary>Must only be called when holding the lock <see cref="gate"/></summary>
        public void Wait()
        {
            if (waiting) throw new InvalidOperationException("Only one waiter is supported!");
            waiting = true;
            gate.Release(); // release the lock
            cv.Wait();      // wait until pulse is called
            gate.Wait();    // re-aquire the lock
            waiting = false;
        }
        
        /// <summary>Must only be called when holding the lock <see cref="gate"/></summary>
        public async Task WaitAsync()
        {
            if (waiting) throw new InvalidOperationException("Only one waiter is supported!");
            waiting = true;
            gate.Release(); // release the lock
            await cv.WaitAsync(); // wait until pulse is called
            gate.Wait();    // re-aquire the lock
            waiting = false;
        }

    }
}
