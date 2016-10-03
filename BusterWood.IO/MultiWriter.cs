using System;
using System.Threading.Tasks;

namespace BusterWood.IO
{
    class MultiWriter : IWriter
    {
        readonly IWriter[] writers;

        public MultiWriter(IWriter[] writers)
        {
            if (writers == null) throw new ArgumentNullException(nameof(writers));
            this.writers = writers;
        }

        public Result Write(Slice<byte> src)
        {
            foreach (var w in writers)
            {
                var res = w.Write(src);
                if (res.Bytes != src.Count)
                {
                    return new Result(0, Writer.ShortWrite);
                }
            }
            return new Result(src.Count, null);
        }

        public async Task<Result> WriteAsync(Slice<byte> src)
        {
            foreach (var w in writers)
            {
                var res = await w.WriteAsync(src);
                if (res.Bytes != src.Count)
                {
                    return new Result(0, Writer.ShortWrite);
                }
            }
            return new Result(src.Count, null);
        }
    }
}