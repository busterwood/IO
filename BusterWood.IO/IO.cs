﻿using System;
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

        /// <summary>Asynchronous read up of <see cref="Block{}.Length"/> bytes into <paramref name="buf"/> from the underlying data stream</summary>
        /// <param name="buf">Where the read data is written too</param>
        /// <returns>the number of <see cref="IOResult.Bytes"/> read and any error that caused read to stop early</returns>
        Task<IOResult> ReadAsync(Block<byte> buf);
    }

    public interface IWriter
    {
        /// <summary>Writes the whole of <paramref name="buf"/> to the underlying data stream</summary>
        /// <param name="buf"></param>
        /// <returns>the number of <see cref="IOResult.Bytes"/> written and any error that caused write to stop early</returns>
        /// <remarks>Implementation must not modify the contents of <paramref name="buf"/></remarks>
        IOResult Write(Block<byte> buf);

        /// <summary>Asynchronous writes the whole of <paramref name="buf"/> to the underlying data stream</summary>
        /// <param name="buf"></param>
        /// <returns>the number of <see cref="IOResult.Bytes"/> written and any error that caused write to stop early</returns>
        /// <remarks>Implementation must not modify the contents of <paramref name="buf"/></remarks>
        Task<IOResult> WriteAsync(Block<byte> buf);
    }

    public interface IReaderAt
    {
        IOResult ReadAt(Block<byte> buf, long offset);
        Task<IOResult> ReadAtAsync(Block<byte> buf, long offset);
    }

    public interface IWriterAt
    {
        IOResult WriteAt(Block<byte> buf, long offset);
    }

    public interface IByteReader
    {
        ByteResult ReadByte();
    }

    public interface IByteScanner : IByteReader
    {
        Exception UnreadByte();
    }

    public interface IByteWriter
    {
        Exception WriteByte(byte b);
    }

    public interface ISeeker
    {
        IOLongResult Seek(long offset, SeekOrigin relativeTo); // there is no async equivalent in Win32
    }

    public interface ICloser
    {
        void Close();   // there is no async equivalent, but it *could* be useful for Pipe
    }

    public interface IReadCloser : IReader, ICloser { }

    public interface IWriteCloser : IWriter, ICloser { }

    public interface IReadSeeker: IReader, ISeeker { }

    public interface IReadSeekCloser: IReadSeeker, IReadCloser { }

    public interface IWriteSeeker: IWriter, ISeeker { }

    public interface IWriteSeekCloser: IWriteSeeker, IWriteCloser { }

    /// <summary>The result of a <see cref="IReader.Read(Block{byte})"/> or <see cref="IWriter.Write(Block{byte})"/></summary>
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

    public struct ByteResult
    {
        public readonly byte Value;
        public readonly Exception Error;

        public ByteResult(byte value, Exception error)
        {
            Value = value;
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

        public static IReader Reader(this Stream stream) => new StreamReader(stream);

        public static IReader Reader(this byte[] data) => new MemoryReader(data);

        public static IReader Reader(this Block<byte> data) => new MemoryReader(data);

        public static IReader Limit(this IReader reader, long limit) => new LimitReader(reader, limit);

        public static IReader MultiReader(params IReader[] readers) => new MultiReader(readers);

        public static IWriter MultiWriter(params IWriter[] writers) => new MultiWriter(writers);

        /// <summary>
        /// TeeReader returns a Reader that writes to <paramref name="to"/> what it reads from <paramref name="from"/>. 
        /// All reads from r performed through it are matched with corresponding writes to w.
        /// There is no internal buffering - the write must complete before the read completes.
        /// </summary>
        /// <remarks>Any error encountered while writing is reported as a read error.</remarks>
        public static IReader Tee(this IReader from, IWriter to) => new TeeReader(from, to);

        /// <summary>Copy copies from src to <paramref name="to"/> until either EOF is reached on <paramref name="from"/> or an error occurs. </summary>
        /// <returns>the number of bytes copied and the first error encountered while copying, if any</returns>
        public static IOLongResult CopyTo(this IReader from, IWriter to) => CopyBuffer(from, to, default(Block<byte>));

        /// <summary>Copy copies from src to <paramref name="to"/> until either EOF is reached on <paramref name="from"/> or an error occurs. </summary>
        /// <returns>the number of bytes copied and the first error encountered while copying, if any</returns>
        public static Task<IOLongResult> CopyToAsync(this IReader from, IWriter to) => CopyBufferAsync(from, to, default(Block<byte>));

        public static IOLongResult CopyBuffer(this IReader from, IWriter to, Block<byte> buf)
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

        public static async Task<IOLongResult> CopyBufferAsync(this IReader from, IWriter to, Block<byte> buf)
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

        public static IOLongResult CopyTo(this IReader from, IWriter to, long bytes)
        {
            var res = CopyTo(Limit(from, bytes), to);
            if (res.Bytes == bytes)
                return new IOLongResult(res.Bytes, null);
            if (res.Bytes < bytes && res.Error == null)
                return new IOLongResult(res.Bytes, EOF);
            return res;
        }

        public static async Task<IOLongResult> CopyToAsync(this IReader from, IWriter to, long bytes)
        {
            var res = await CopyToAsync(Limit(from, bytes), to);
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