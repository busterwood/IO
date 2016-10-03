using System;
using System.IO;
using System.Threading.Tasks;

namespace BusterWood.IO
{
    class StreamReaderWrapper : IReader
    {
        Stream stream;

        public StreamReaderWrapper(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            this.stream = stream;
        }

        public Result Read(Slice<byte> dest)
        {
            try
            {
                int bytes = stream.Read(dest.Array, dest.Offset, dest.Count);
                return new Result(bytes, bytes == 0 ? Reader.EOF : null);
            }
            catch (IOException ex)
            {
                return new Result(0, ex);
            }
            catch (ObjectDisposedException ex)
            {
                return new Result(0, ex);
            }
        }

        public async Task<Result> ReadAsync(Slice<byte> dest)
        {
            try
            {
                int bytes = await stream.ReadAsync(dest.Array, dest.Offset, dest.Count);
                return new Result(bytes, bytes == 0 ? Reader.EOF : null);
            }
            catch (IOException ex)
            {
                return new Result(0, ex);
            }
            catch (ObjectDisposedException ex)
            {
                return new Result(0, ex);
            }
        }
    }
}