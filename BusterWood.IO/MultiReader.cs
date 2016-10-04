using System.Threading.Tasks;

namespace BusterWood
{
    class MultiReader : IReader
    {
        Slice<IReader> readers;

        public MultiReader(Slice<IReader> readers)
        {
            this.readers = readers;
        }

        public IOResult Read(Slice<byte> dest)
        {
            while (readers.Length > 0)
            {
                if (readers.Length == 1)
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
                if (result.Bytes > 0 || result.Error != IO.EOF)
                {
                    if (result.Error == IO.EOF)
                    {
                        // dont return EOF yet, there may be more bytes in the remaining readers
                        return new IOResult(result.Bytes, null);
                    }
                    return result;
                }
                readers = readers.SubSlice(1);
            }
            return new IOResult(0, IO.EOF);
        }

        public async Task<IOResult> ReadAsync(Slice<byte> dest)
        {
            while (readers.Length > 0)
            {
                if (readers.Length == 1)
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
                if (result.Bytes > 0 || result.Error != IO.EOF)
                {
                    if (result.Error == IO.EOF)
                    {
                        // dont return EOF yet, there may be more bytes in the remaining readers
                        return new IOResult(result.Bytes, null);
                    }
                    return result;
                }
                readers = readers.SubSlice(1);
            }
            return new IOResult(0, IO.EOF);
        }
    }
}