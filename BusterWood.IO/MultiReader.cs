using System.Threading.Tasks;

namespace BusterWood.InputOutput
{
    public class MultiReader : IReader
    {
        Block<IReader> readers;

        public MultiReader(Block<IReader> readers)
        {
            this.readers = readers;
        }

        public IOResult Read(Block<byte> dest)
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
                if (result.Bytes > 0 || result.Error != Io.EOF)
                {
                    if (result.Error == Io.EOF)
                    {
                        // dont return EOF yet, there may be more bytes in the remaining readers
                        return new IOResult(result.Bytes, null);
                    }
                    return result;
                }
                readers = readers.Slice(1);
            }
            return new IOResult(0, Io.EOF);
        }

        public async Task<IOResult> ReadAsync(Block<byte> dest)
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
                if (result.Bytes > 0 || result.Error != Io.EOF)
                {
                    if (result.Error == Io.EOF)
                    {
                        // dont return EOF yet, there may be more bytes in the remaining readers
                        return new IOResult(result.Bytes, null);
                    }
                    return result;
                }
                readers = readers.Slice(1);
            }
            return new IOResult(0, Io.EOF);
        }
    }
}