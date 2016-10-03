using System.Threading.Tasks;

namespace BusterWood.IO
{

    class MultiReader : IReader
    {
        Slice<IReader> readers;

        public MultiReader(Slice<IReader> readers)
        {
            this.readers = readers;
        }

        public Result Read(Slice<byte> dest)
        {
            while (readers.Count > 0)
            {
                if (readers.Count == 1)
                {
                    // try to flatten nested multi readers
                    var r = readers[0] as MultiReader;
                    if (r != null)
                    {
                        readers = r.readers;
                        continue;
                    }
                }

                var result = readers[0].Read(dest);
                if (result.Bytes > 0 || result.Error != Reader.EOF)
                {
                    if (result.Error == Reader.EOF)
                    {
                        // dont return EOF yet, there may be more bytes in the remaining readers
                        return new Result(result.Bytes, null);
                    }
                    return result;
                }
                readers = readers.SubSlice(1);
            }
            return new Result(0, Reader.EOF);
        }

        public async Task<Result> ReadAsync(Slice<byte> dest)
        {
            while (readers.Count > 0)
            {
                if (readers.Count == 1)
                {
                    // try to flatten nested multi readers
                    var r = readers[0] as MultiReader;
                    if (r != null)
                    {
                        readers = r.readers;
                        continue;
                    }
                }

                var result = await readers[0].ReadAsync(dest);
                if (result.Bytes > 0 || result.Error != Reader.EOF)
                {
                    if (result.Error == Reader.EOF)
                    {
                        // dont return EOF yet, there may be more bytes in the remaining readers
                        return new Result(result.Bytes, null);
                    }
                    return result;
                }
                readers = readers.SubSlice(1);
            }
            return new Result(0, Reader.EOF);
        }
    }
}