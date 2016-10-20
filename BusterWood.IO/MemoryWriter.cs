using System.Threading.Tasks;

namespace BusterWood.InputOutput
{
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