using System;
using System.Threading.Tasks;

namespace BusterWood.InputOutput
{
    class LimitReader : IReader
    {
        readonly IReader reader;
        long bytesRemaining;

        public LimitReader(IReader reader, long limit)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (limit < 0) throw new ArgumentOutOfRangeException(nameof(limit), "cannot be negative");
            this.reader = reader;
            bytesRemaining = limit;
        }

        public IOResult Read(Slice<byte> dest)
        {
            if (bytesRemaining <= 0)
                return new IOResult(0, IO.EOF);
            if (dest.Length > bytesRemaining)
                dest = dest.SubSlice(0, (int)bytesRemaining);
            var res = reader.Read(dest);
            bytesRemaining -= res.Bytes;
            return res;
        }

        public async Task<IOResult> ReadAsync(Slice<byte> dest)
        {
            if (bytesRemaining <= 0)
                return new IOResult(0, IO.EOF);
            if (dest.Length > bytesRemaining)
                dest = dest.SubSlice(0, (int)bytesRemaining);
            var res = await reader.ReadAsync(dest);
            bytesRemaining -= res.Bytes;
            return res;
        }
    }
}