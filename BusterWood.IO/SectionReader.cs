using System;
using System.IO;
using System.Threading.Tasks;

namespace BusterWood.InputOutput
{
    /// <summary>
    /// SectionReader implements Read, Seek, and ReadAt on a section of an underlying ReaderAt.
    /// </summary>
    public class SectionReader : IReadSeeker, IReaderAt
    {
        static readonly Exception errOrigin = new IOException("Seek: invalid SeekOrigin");
        static readonly Exception errOffset = new IOException("Seek: invalid offset");
        readonly IReaderAt inner;
        long start;
        long offset;
        readonly long end;

        /// <summary>
        /// returns a SectionReader that reads from <param name="inner"/> starting at offset <paramref name="offset"/> and stops with EOF after <paramref name="length"/> bytes.
        /// </summary>
        public SectionReader(IReaderAt inner, long offset, long length)
        {
            this.inner = inner;
            start = offset;
            this.offset = offset;
            this.end = start + length;
        }

        public IOResult Read(Block<byte> buf)
        {
            if (offset >= end)
                return new IOResult(0, Io.EOF);
            long max = end - offset;
            if (buf.Length > max)
                buf = buf.Slice(0, (int)max);
            var res = inner.ReadAt(buf, offset);
            offset += res.Bytes;
            return res;
        }

        public async Task<IOResult> ReadAsync(Block<byte> buf)
        {
            if (offset >= end)
                return new IOResult(0, Io.EOF);
            long max = end - offset;
            if (buf.Length > max)
                buf = buf.Slice(0, (int)max);
            var res = await inner.ReadAtAsync(buf, offset);
            offset += res.Bytes;
            return res;
        }

        public IOLongResult Seek(long offset, SeekOrigin relativeTo)
        {
            switch (relativeTo)
            {
                case SeekOrigin.Begin:
                    offset += start;
                    break;
                case SeekOrigin.Current:
                    offset += this.offset;
                    break;
                case SeekOrigin.End:
                    offset += end;
                    break;
                default:
                    return new IOLongResult(0, errOrigin);
            }
            if (offset < start)
                return new IOLongResult(0, errOffset);
            this.offset = offset;
            return new IOLongResult(offset - start, null);
        }

        public IOResult ReadAt(Block<byte> buf, long off)
        {
            if (off < 0 || off >= end - start)
                return new IOResult(0, Io.EOF);
            off += start;
            long max = end - off;
            if (buf.Length > max)
            {
                buf = buf.Slice(0, (int)max);
                var res = inner.ReadAt(buf, off);
                if (res.Error == null)
                    return new IOResult(res.Bytes, Io.EOF);
                return res;
            }
            return inner.ReadAt(buf, off);
        }

        public async Task<IOResult> ReadAtAsync(Block<byte> buf, long off)
        {
            if (off < 0 || off >= end - start)
                return new IOResult(0, Io.EOF);
            off += start;
            long max = end - off;
            if (buf.Length > max)
            {
                buf = buf.Slice(0, (int)max);
                var res = await inner.ReadAtAsync(buf, off);
                if (res.Error == null)
                    return new IOResult(res.Bytes, Io.EOF);
                return res;
            }
            return await inner.ReadAtAsync(buf, off);
        }

        // Size returns the size of the section in bytes.
        public long Size() => end - start;

    }
}
