using System;
using System.Collections;
using System.Collections.Generic;

namespace BusterWood.InputOutput
{
    /// <summary>The whole or a part of an <see cref="Array"/></summary>
    public struct Slice<T> : IReadOnlyList<T>
    {
        readonly T[] _array;
        readonly int _start; // inclusive
        readonly int _end; // exclusive

        public Slice(T[] array)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            _array = array;
            _start = 0;
            _end = array.Length;
        }

        public Slice(T[] array, int start, int end)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start), "cannot be negative");
            if (end < 0) throw new ArgumentOutOfRangeException(nameof(end), "cannot be negative");
            if (end > array.Length) throw new ArgumentOutOfRangeException(nameof(end), "past the end of array");
            _array = array;
            _start = start;
            _end = end;
        }

        public T[] Array => _array;

        public int Offset => _start;

        public int Length => _end - _start;

        int IReadOnlyCollection<T>.Count => _end - _start;

        public T this[int index]
        {
            get
            {
                if (_array == null) throw new InvalidOperationException("Backing array is null");
                int i = _start + index;
                if (index < 0 || i >= _end) throw new ArgumentOutOfRangeException(nameof(index));
                return _array[i];
            }
        }

        public int CopyTo(Slice<byte> buf)
        {
            int bytes = buf.Length < Length ? buf.Length : Length;
            if (bytes > 0)
                System.Array.Copy(_array, _start, buf._array, buf._start, bytes);
            return bytes;
        }

        public override int GetHashCode() => null == _array ? 0 : _array.GetHashCode() ^ _start ^ _end;

        public override bool Equals(object obj) => obj is Slice<T> ? Equals((Slice<T>)obj) : false;

        public bool Equals(Slice<T> obj) => obj._array == _array && obj._start == _start && obj._end == _end;

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = _start; i < _end; i++)
                yield return _array[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Slice<T> SubSlice(int start)
        {
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start), "cannot be negative");
            return new Slice<T>(_array, _start + start, _end);
        }

        public Slice<T> SubSlice(int start, int end)
        {
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start), "cannot be negative");
            if (end < start) throw new ArgumentOutOfRangeException(nameof(end), "cannot less than start");
            if (end > _end) throw new ArgumentOutOfRangeException(nameof(end), "past the end");
            return new Slice<T>(_array, _start + start, _start + end);
        }

        public static bool operator ==(Slice<T> a, Slice<T> b) => a.Equals(b);

        public static bool operator !=(Slice<T> a, Slice<T> b) => !(a == b);

        public static implicit operator Slice<T>(T[] array) => new Slice<T>(array);
    }
}