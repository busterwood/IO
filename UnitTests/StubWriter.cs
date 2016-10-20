using BusterWood.InputOutput;
using System.Threading.Tasks;
using System;

namespace UnitTests
{

    class StubWriter : IWriter
    {
        public int? Limit;
        public Exception Error;

        public IOResult Write(Block<byte> buf)
        {
            if (Limit.HasValue)
                return new IOResult(Limit.Value, Error);
            return new IOResult(0, Error);
        }

        public Task<IOResult> WriteAsync(Block<byte> buf)
        {
            if (Limit.HasValue)
                return Task.FromResult(new IOResult(Limit.Value, Error));
            return Task.FromResult(new IOResult(0, Error));
        }
    }
}