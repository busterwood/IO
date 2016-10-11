using System;
using System.Threading.Tasks;

namespace BusterWood.InputOutput
{
    class TeeReader : IReader
    {
        readonly IWriter dst;
        readonly IReader src;

        public TeeReader(IReader src, IWriter dst)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));
            if (dst == null)
                throw new ArgumentNullException(nameof(dst));
            this.src = src;
            this.dst = dst;
        }

        public IOResult Read(Block<byte> buf)
        {
            var rr = src.Read(buf);
            if (rr.Bytes > 0)
            {
                var wr = dst.Write(buf.Slice(0, rr.Bytes));
                if (wr.Error != null)
                    return wr;
            }
            return rr;
        }

        public async Task<IOResult> ReadAsync(Block<byte> buf)
        {
            var rr = await src.ReadAsync(buf);
            if (rr.Bytes > 0)
            {
                var wr = await dst.WriteAsync(buf.Slice(0, rr.Bytes));
                if (wr.Error != null)
                    return wr;
            }
            return rr;
        }
    }
}