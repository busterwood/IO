using System;
using System.IO;
using System.Threading.Tasks;

namespace BusterWood
{
    public interface IReader
    {
        /// <summary>Read up of <see cref="Slice{}.Length"/> bytes into <paramref name="dest"/> from the underlying data stream</summary>
        /// <param name="dest">Where the read data is written too</param>
        /// <returns>the number of <see cref="IOResult.Bytes"/> read and any error that caused read to stop early</returns>
        IOResult Read(Slice<byte> dest);

        Task<IOResult> ReadAsync(Slice<byte> dest);
    }

    public interface IWriter
    {
        /// <summary>Writes the whole of <paramref name="src"/> to the underlying data stream</summary>
        /// <param name="src"></param>
        /// <returns>the number of <see cref="IOResult.Bytes"/> written and any error that caused write to stop early</returns>
        /// <remarks>Implementation must not modify the contents of <paramref name="src"/></remarks>
        IOResult Write(Slice<byte> src);

        Task<IOResult> WriteAsync(Slice<byte> src);
    }

    /// <summary>The result of a <see cref="IReader.Read(Slice{byte})"/> or <see cref="IWriter.Write(Slice{byte})"/></summary>
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

    public static class IO
    {
        internal static readonly EndOfStreamException EOF = new EndOfStreamException();
        internal static readonly IOException ShortWrite = new IOException("Short write error");

        public static IReader ReaderFromStream(Stream stream) => new StreamReaderWrapper(stream);

        public static IReader MultiReader(params IReader[] readers) => new MultiReader(readers);

        public static IReader LimitReader(IReader reader, long limit) => new LimitReader(reader, limit);

        public static IWriter MultiWriter(params IWriter[] writers) => new MultiWriter(writers);

        /// <summary>
        /// TeeReader returns a Reader that writes to <paramref name="dst"/> what it reads from <paramref name="src"/>. 
        /// All reads from r performed through it are matched with corresponding writes to w.
        /// There is no internal buffering - the write must complete before the read completes.
        /// </summary>
        /// <remarks>Any error encountered while writing is reported as a read error.</remarks>
        public static IReader Tee(IReader src, IWriter dst) => new TeeReader(src, dst);

        /// <summary>Copy copies from src to <paramref name="dst"/> until either EOF is reached on <paramref name="src"/> or an error occurs. </summary>
        /// <returns>the number of bytes copied and the first error encountered while copying, if any</returns>
        public static IOLongResult Copy(IWriter dst, IReader src) => CopyBuffer(dst, src, null);

        /// <summary>Copy copies from src to <paramref name="dst"/> until either EOF is reached on <paramref name="src"/> or an error occurs. </summary>
        /// <returns>the number of bytes copied and the first error encountered while copying, if any</returns>
        public static Task<IOLongResult> CopyAsync(IWriter dst, IReader src) => CopyBufferAsync(dst, src, null);

        public static IOLongResult CopyBuffer(IWriter dst, IReader src, Slice<byte> buf)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));
            if (dst == null)
                throw new ArgumentNullException(nameof(dst));
            if (buf.Length == 0)
                throw new ArgumentException("buffer must be 1 or more bytes", nameof(buf));

            if (buf == null)
                buf = new byte[32 * 1024];

            long written = 0;
            for (;;)
            {
                var rr = src.Read(buf);
                if (rr.Bytes > 0)
                {
                    var wr = dst.Write(buf.SubSlice(0, rr.Bytes));
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

        public static async Task<IOLongResult> CopyBufferAsync(IWriter dst, IReader src, Slice<byte> buf)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));
            if (dst == null)
                throw new ArgumentNullException(nameof(dst));
            if (buf.Length == 0)
                throw new ArgumentException("buffer must be 1 or more bytes", nameof(buf));

            if (buf == null)
                buf = new byte[32 * 1024];

            long written = 0;
            for (;;)
            {
                var rr = await src.ReadAsync(buf);
                if (rr.Bytes > 0)
                {
                    var wr = await dst.WriteAsync(buf.SubSlice(0, rr.Bytes));
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

        public static IOLongResult CopyN(IWriter dst, IReader src, long bytes)
        {
            var res = Copy(dst, LimitReader(src, bytes));
            if (res.Bytes == bytes)
                return new IOLongResult(res.Bytes, null);
            if (res.Bytes < bytes && res.Error == null)
                return new IOLongResult(res.Bytes, EOF);
            return res;
        }

        public static async Task<IOLongResult> CopyNAsync(IWriter dst, IReader src, long bytes)
        {
            var res = await CopyAsync(dst, LimitReader(src, bytes));
            if (res.Bytes == bytes)
                return new IOLongResult(res.Bytes, null);
            if (res.Bytes < bytes && res.Error == null)
                return new IOLongResult(res.Bytes, EOF);
            return res;
        }
    }


}