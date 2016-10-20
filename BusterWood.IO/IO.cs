using System;
using System.IO;
using System.Threading.Tasks;

namespace BusterWood.InputOutput
{
    public interface IReader
    {
        /// <summary>Read up of <see cref="Block{}.Length"/> bytes into <paramref name="buf"/> from the underlying data stream</summary>
        /// <param name="buf">Where the read data is written too</param>
        /// <returns>the number of <see cref="IOResult.Bytes"/> read and any error that caused read to stop early</returns>
        IOResult Read(Block<byte> buf);

        Task<IOResult> ReadAsync(Block<byte> buf);
    }

    public interface IWriter
    {
        /// <summary>Writes the whole of <paramref name="buf"/> to the underlying data stream</summary>
        /// <param name="buf"></param>
        /// <returns>the number of <see cref="IOResult.Bytes"/> written and any error that caused write to stop early</returns>
        /// <remarks>Implementation must not modify the contents of <paramref name="buf"/></remarks>
        IOResult Write(Block<byte> buf);

        Task<IOResult> WriteAsync(Block<byte> buf);
    }

    public interface ICloser
    {
        void Close();
    }

    public interface IReadCloser : IReader, ICloser { }

    public interface IWriteCloser : IWriter, ICloser { }

    /// <summary>The result of a <see cref="IReader.Read(Block{byte})"/> or <see cref="IWriteCloser.Write(Block{byte})"/></summary>
    public struct IOResult
    {
        public readonly int Bytes;
        public readonly Exception Error;

        public IOResult(int bytes, Exception error)
        {
            Bytes = bytes;
            Error = error;
        }
    }

    public struct IOLongResult
    {
        public readonly long Bytes;
        public readonly Exception Error;

        public IOLongResult(long bytes, Exception error)
        {
            Bytes = bytes;
            Error = error;
        }
    }

    public static class Io
    {
        public static readonly EndOfStreamException EOF = new EndOfStreamException();

        public static readonly IOException ShortWrite = new IOException("Short write error");

        public static IReader Reader(Stream stream) => new StreamReaderWrapper(stream);

        public static IReader Reader(Block<byte> data) => new MemoryReader(data);

        public static IReader MultiReader(params IReader[] readers) => new MultiReader(readers);

        public static IReader LimitReader(IReader reader, long limit) => new LimitReader(reader, limit);

        public static IWriter MultiWriter(params IWriter[] writers) => new MultiWriter(writers);

        /// <summary>
        /// TeeReader returns a Reader that writes to <paramref name="to"/> what it reads from <paramref name="from"/>. 
        /// All reads from r performed through it are matched with corresponding writes to w.
        /// There is no internal buffering - the write must complete before the read completes.
        /// </summary>
        /// <remarks>Any error encountered while writing is reported as a read error.</remarks>
        public static IReader Tee(IReader from, IWriter to) => new TeeReader(from, to);

        /// <summary>Copy copies from src to <paramref name="to"/> until either EOF is reached on <paramref name="from"/> or an error occurs. </summary>
        /// <returns>the number of bytes copied and the first error encountered while copying, if any</returns>
        public static IOLongResult Copy(IReader from, IWriter to) => CopyBuffer(from, to, default(Block<byte>));

        /// <summary>Copy copies from src to <paramref name="to"/> until either EOF is reached on <paramref name="from"/> or an error occurs. </summary>
        /// <returns>the number of bytes copied and the first error encountered while copying, if any</returns>
        public static Task<IOLongResult> CopyAsync(IReader from, IWriter to) => CopyBufferAsync(from, to, default(Block<byte>));

        public static IOLongResult CopyBuffer(IReader from, IWriter to, Block<byte> buf)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));
            if (to == null)
                throw new ArgumentNullException(nameof(to));

            if (buf.Length == 0)
                buf = new byte[32 * 1024];

            long written = 0;
            for (;;)
            {
                var rr = from.Read(buf);
                if (rr.Bytes > 0)
                {
                    var wr = to.Write(buf.Slice(0, rr.Bytes));
                    if (wr.Bytes > 0)
                        written += wr.Bytes;
                    if (wr.Error != null)
                        return new IOLongResult(written, wr.Error);
                    if (wr.Bytes != rr.Bytes)
                        return new IOLongResult(written, ShortWrite);
                }
                if (rr.Error == EOF)
                    return new IOLongResult(written, null);
                if (rr.Error != null)
                    return new IOLongResult(written, rr.Error);
            }
        }

        public static async Task<IOLongResult> CopyBufferAsync(IReader from, IWriter to, Block<byte> buf)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));
            if (to == null)
                throw new ArgumentNullException(nameof(to));
            if (buf.Length == 0)
                throw new ArgumentException("buffer must be 1 or more bytes", nameof(buf));

            if (buf == null)
                buf = new byte[32 * 1024];

            long written = 0;
            for (;;)
            {
                var rr = await from.ReadAsync(buf);
                if (rr.Bytes > 0)
                {
                    var wr = await to.WriteAsync(buf.Slice(0, rr.Bytes));
                    if (wr.Bytes > 0)
                        written += wr.Bytes;
                    if (wr.Error != null)
                        return new IOLongResult(written, wr.Error);
                    if (wr.Bytes != rr.Bytes)
                        return new IOLongResult(written, ShortWrite);
                }
                if (rr.Error == EOF)
                    return new IOLongResult(written, null);
                if (rr.Error != null)
                    return new IOLongResult(written, rr.Error);
            }
        }

        public static IOLongResult CopyN(IReader from, IWriter to, long bytes)
        {
            var res = Copy(LimitReader(from, bytes), to);
            if (res.Bytes == bytes)
                return new IOLongResult(res.Bytes, null);
            if (res.Bytes < bytes && res.Error == null)
                return new IOLongResult(res.Bytes, EOF);
            return res;
        }

        public static async Task<IOLongResult> CopyNAsync(IReader from, IWriter to, long bytes)
        {
            var res = await CopyAsync(LimitReader(from, bytes), to);
            if (res.Bytes == bytes)
                return new IOLongResult(res.Bytes, null);
            if (res.Bytes < bytes && res.Error == null)
                return new IOLongResult(res.Bytes, EOF);
            return res;
        }

        public static PipeEnds Pipe()
        {
            var p = new Pipe();
            return new PipeEnds(new PipeReader(p), new PipeWriter(p));
        }
    }

    public struct PipeEnds
    {
        public PipeReader Reader { get; }
        public PipeWriter Writer { get; }

        public PipeEnds(PipeReader reader, PipeWriter writer)
        {
            Reader = reader;
            Writer = writer;
        }
    }
}