using System;
using System.Threading.Tasks;

namespace BusterWood.IO
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

        public Result Read(Slice<byte> dest)
        {
            if (bytesRemaining <= 0)
                return new Result(0, Reader.EOF);
            if (dest.Count > bytesRemaining)
                dest = dest.SubSlice(0, (int)bytesRemaining);
            var res = reader.Read(dest);
            bytesRemaining -= res.Bytes;
            return res;
        }

        public async Task<Result> ReadAsync(Slice<byte> dest)
        {
            if (bytesRemaining <= 0)
                return new Result(0, Reader.EOF);
            if (dest.Count > bytesRemaining)
                dest = dest.SubSlice(0, (int)bytesRemaining);
            var res = await reader.ReadAsync(dest);
            bytesRemaining -= res.Bytes;
            return res;
        }
    }
}