using System;
using System.Threading.Tasks;

namespace BusterWood.InputOutput
{
    public class MultiWriter : IWriter
    {
        readonly IWriter[] writers;

        public MultiWriter(IWriter[] writers)
        {
            if (writers == null) throw new ArgumentNullException(nameof(writers));
            this.writers = writers;
        }

        public IOResult Write(Block<byte> src)
        {
            foreach (var w in writers)
            {
                var res = w.Write(src);
                if (res.Bytes != src.Length)
                {
                    return new IOResult(res.Bytes, Io.ShortWrite);
                }
            }
            return new IOResult(src.Length, null);
        }

        public async Task<IOResult> WriteAsync(Block<byte> src)
        {
            foreach (var w in writers)
            {
                var res = await w.WriteAsync(src);
                if (res.Bytes != src.Length)
                {
                    return new IOResult(res.Bytes, Io.ShortWrite);
                }
            }
            return new IOResult(src.Length, null);
        }
    }
}