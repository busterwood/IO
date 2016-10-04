using System;
using System.IO;
using System.Threading.Tasks;

namespace BusterWood
{
    class StreamReaderWrapper : IReader
    {
        Stream stream;

        public StreamReaderWrapper(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            this.stream = stream;
        }

        public IOResult Read(Slice<byte> dest)
        {
            try
            {
                int bytes = stream.Read(dest.Array, dest.Offset, dest.Length);
                return new IOResult(bytes, bytes == 0 ? IO.EOF : null);
            }
            catch (IOException ex)
            {
                return new IOResult(0, ex);
            }
            catch (ObjectDisposedException ex)
            {
                return new IOResult(0, ex);
            }
        }

        public async Task<IOResult> ReadAsync(Slice<byte> dest)
        {
            try
            {
                int bytes = await stream.ReadAsync(dest.Array, dest.Offset, dest.Length);
                return new IOResult(bytes, bytes == 0 ? IO.EOF : null);
            }
            catch (IOException ex)
            {
                return new IOResult(0, ex);
            }
            catch (ObjectDisposedException ex)
            {
                return new IOResult(0, ex);
            }
        }
    }
}