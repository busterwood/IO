using System;
using System.Collections;
using System.Collections.Generic;

namespace BusterWood.InputOutput
{
    /// <summary>The whole or a part of an <see cref="Array"/></summary>
    public struct Block<T> : IReadOnlyList<T>
    {
        readonly T[] _array;
        readonly int _start; // inclusive
        readonly int _end; // exclusive

        public Block(int size) : this(new T[size]) { }

        public Block(int size, int capacity) : this(new T[capacity], 0, size) { }

        public Block(T[] array)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            _array = array;
            _start = 0;
            _end = array.Length;
        }

        public Block(T[] array, int start, int end)
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

        public int Capacity => _array.Length - _end;

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
            set
            {
                if (_array == null) throw new InvalidOperationException("Backing array is null");
                int i = _start + index;
                if (index < 0 || i >= _end) throw new ArgumentOutOfRangeException(nameof(index));
                _array[i] = value;
            }
        }

        public int CopyTo(Block<T> buf)
        {
            int bytes = buf.Length < Length ? buf.Length : Length;
            if (bytes > 0)
                System.Array.Copy(_array, _start, buf._array, buf._start, bytes);
            return bytes;
        }

        public override int GetHashCode() => null == _array ? 0 : _array.GetHashCode() ^ _start ^ _end;

        public override bool Equals(object obj) => obj is Block<T> ? Equals((Block<T>)obj) : false;

        public bool Equals(Block<T> obj) => obj._array == _array && obj._start == _start && obj._end == _end;

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = _start; i < _end; i++)
                yield return _array[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Block<T> Slice(int start)
        {
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start), "cannot be negative");
            return new Block<T>(_array, _start + start, _end);
        }

        public Block<T> Slice(int start, int end)
        {
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start), "cannot be negative");
            if (end < start) throw new ArgumentOutOfRangeException(nameof(end), "cannot less than start");
            if (end > _array.Length) throw new ArgumentOutOfRangeException(nameof(end), "past the end");
            return new Block<T>(_array, _start + start, _start + end);
        }

        public static bool operator ==(Block<T> a, Block<T> b) => a.Equals(b);

        public static bool operator !=(Block<T> a, Block<T> b) => !(a == b);

        public static implicit operator Block<T>(T[] array) => new Block<T>(array);
    }

    public static partial class Extensions
    {
        public static Block<T> Slice<T>(this T[] array, int start) => new Block<T>(array, start, array.Length);

        public static Block<T> Slice<T>(this T[] array, int start, int end) => new Block<T>(array, start, end);

        /// <summary>
        /// The Append function appends elements to the end of a <see cref="Block{T}"/>. 
        /// If it has sufficient Capacity, the <param name="block"/> is adjusted to accommodate the new elements. 
        /// If it does not, a new underlying array will be allocated.
        /// Append returns the updated <see cref="Block{T}"/>. It is therefore necessary to store the
        /// result of Append, often in the variable holding the slice itself.
        /// </summary>
        /// <example>
        ///	block = block.Append(elem1, elem2);
        /// </example>
        public static Block<T> Append<T>(this Block<T> block, params T[] data) => Append(block, new Block<T>(data));

        /// <summary>
        /// The Append function appends elements to the end of a <see cref="Block{T}"/>. 
        /// If it has sufficient Capacity, the <param name="block"/> is adjusted to accommodate the new elements. 
        /// If it does not, a new underlying array will be allocated.
        /// Append returns the updated <see cref="Block{T}"/>. It is therefore necessary to store the
        /// result of Append, often in the variable holding the slice itself.
        /// </summary>
        /// <example>
        ///	block = block.Append(anotherBlock);
        /// </example>
        public static Block<T> Append<T>(this Block<T> block, Block<T> other)
        {
            var m = block.Length;
            var newLen = m + other.Length;
            if (newLen > block.Capacity) // if necessary, reallocate
            {
                Block<T> newBlock = new T[(newLen + 1) * 2]; // allocate double what's needed, for future growth.
                block.CopyTo(newBlock);
                block = newBlock;
            }
            block = block.Slice(0, newLen);
            other.CopyTo(block.Slice(m, newLen));
            return block;
        }
    }
}