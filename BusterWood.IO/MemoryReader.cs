using System.Threading.Tasks;

namespace BusterWood.InputOutput
{
    public class MemoryReader : IReader
    {
        Block<byte> _data;

        public MemoryReader(Block<byte> data)
        {
            _data = data;
        }

        public IOResult Read(Block<byte> buf)
        {
            int bytesCopied = _data.CopyTo(buf);
            if (bytesCopied == 0)
                return new IOResult(0, Io.EOF);
            _data = _data.Slice(bytesCopied);
            return new IOResult(bytesCopied, null);
        }

        public Task<IOResult> ReadAsync(Block<byte> buf)
        {
            int bytesCopied = _data.CopyTo(buf);
            if (bytesCopied == 0)
                return Task.FromResult(new IOResult(0, Io.EOF));
            _data = _data.Slice(bytesCopied);
            return Task.FromResult(new IOResult(bytesCopied, null));
        }
    }
}
