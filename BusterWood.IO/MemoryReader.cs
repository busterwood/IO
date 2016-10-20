using System;
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

    public class MemoryWriter : IWriter
    {
        Block<byte> _data;

        public Block<byte> Data => _data;
        
        public MemoryWriter(Block<byte> data)
        {
            _data = data;
        }

        public IOResult Write(Block<byte> buf)
        {
            _data = _data.Append(buf);
            return new IOResult(buf.Length, null);
        }

        public Task<IOResult> WriteAsync(Block<byte> buf)
        {
            _data = _data.Append(buf);
            return Task.FromResult(new IOResult(buf.Length, null));
        }
    }
}
