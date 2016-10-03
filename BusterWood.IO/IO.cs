using System;
using System.IO;
using System.Threading.Tasks;

namespace BusterWood.IO
{
    public interface IReader
    {
        /// <summary>Read up of <see cref="Slice{}.Count"/> bytes into <paramref name="dest"/> from the underlying data stream</summary>
        /// <param name="dest">Where the read data is written too</param>
        /// <returns>the number of <see cref="Result.Bytes"/> read and any error that caused read to stop early</returns>
        Result Read(Slice<byte> dest);

        Task<Result> ReadAsync(Slice<byte> dest);
    }

    public interface IWriter
    {
        /// <summary>Writes the whole of <paramref name="src"/> to the underlying data stream</summary>
        /// <param name="src"></param>
        /// <returns>the number of <see cref="Result.Bytes"/> written and any error that caused write to stop early</returns>
        /// <remarks>Implementation must not modify the contents of <paramref name="src"/></remarks>
        Result Write(Slice<byte> src);

        Task<Result> WriteAsync(Slice<byte> src);
    }

    /// <summary>The result of a <see cref="IReader.Read(Slice{byte})"/> or <see cref="IWriter.Write(Slice{byte})"/></summary>
    public struct Result
    {
        public readonly int Bytes;
        public readonly Exception Error;

        public Result(int bytes, Exception error)
        {
            Bytes = bytes;
            Error = error;
        }
    }

    public static class Reader
    {
        internal static readonly EndOfStreamException EOF = new EndOfStreamException();

        public static IReader FromStream(Stream stream) => new StreamReaderWrapper(stream);

        public static IReader Multi(params IReader[] readers) => new MultiReader(readers);

        public static IReader Limit(IReader reader, long limit) => new LimitReader(reader, limit);

    }

    public static class Writer
    {
        internal static readonly IOException ShortWrite = new IOException("Short write error");

        public static IWriter Multi(params IWriter[] writers) => new MultiWriter(writers);
    }
}