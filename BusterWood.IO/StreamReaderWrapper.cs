﻿using System;
using System.IO;
using System.Threading.Tasks;

namespace BusterWood.InputOutput
{
    public class StreamReader : IReader
    {
        Stream stream;

        public StreamReader(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            this.stream = stream;
        }

        public IOResult Read(Block<byte> dest)
        {
            try
            {
                int bytes = stream.Read(dest.Array, dest.Offset, dest.Length);
                return new IOResult(bytes, bytes == 0 ? Io.EOF : null);
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

        public async Task<IOResult> ReadAsync(Block<byte> dest)
        {
            try
            {
                int bytes = await stream.ReadAsync(dest.Array, dest.Offset, dest.Length);
                return new IOResult(bytes, bytes == 0 ? Io.EOF : null);
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