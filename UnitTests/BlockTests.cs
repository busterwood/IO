using BusterWood.InputOutput;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture]
    public class BlockTests
    {
        [Test]
        public void empty_block_has_length_zero()
        {
            Assert.AreEqual(0, new Block<byte>().Length);
        }

        [TestCase(0, 0, 0)]
        [TestCase(0, 1, 1)]
        [TestCase(1, 1, 0)]
        [TestCase(1, 2, 1)]
        [TestCase(1, 3, 2)]
        [TestCase(2, 3, 1)]
        public void block_has_length(int start, int end, int expectedLength)
        {
            var arr = new byte[] { 1, 2, 3 };
            var b = new Block<byte>(arr, start, end);
            Assert.AreEqual(expectedLength, b.Length);
            Assert.AreSame(arr, b.Array);
        }

        [TestCase(0, 1, 1)]
        [TestCase(1, 2, 2)]
        [TestCase(2, 3, 3)]
        public void block_have_first_value(int start, int end, byte expected)
        {
            var arr = new byte[] { 1, 2, 3 };
            var b = new Block<byte>(arr, start, end);
            Assert.AreEqual(expected, b[0]);
            Assert.AreSame(arr, b.Array);
        }

        [TestCase(0, 1, 1)]
        [TestCase(1, 2, 2)]
        [TestCase(2, 3, 3)]
        public void can_slice_an_array_create_a_block_that_aliases_the_underlying_array(int start, int end, byte expected)
        {
            var arr = new byte[] { 1, 2, 3 };
            var b = arr.Slice(start, end);
            Assert.AreEqual(expected, b[0]);
            Assert.AreSame(arr, b.Array);
        }

        [TestCase(0, 1)]
        [TestCase(1, 2)]
        [TestCase(2, 3)]
        public void can_slice_an_array_create_a_block_that_aliases_the_underlying_array(int start, byte expected)
        {
            var arr = new byte[] { 1, 2, 3 };
            var b = arr.Slice(start);
            Assert.AreEqual(expected, b[0]);
            Assert.AreSame(arr, b.Array);
        }

        [TestCase(0, 1, 2)]
        [TestCase(1, 2, 3)]
        [TestCase(2, 3, 4)]
        public void can_slice_an_block_and_create_a_block_that_aliases_the_underlying_block(int start, int end, byte expected)
        {
            var arr = new byte[] { 1, 2, 3, 4 };
            var original = new Block<byte>(arr, 1, 4);
            var b = original.Slice(start, end);
            Assert.AreEqual(expected, b[0]);
            Assert.AreSame(arr, b.Array);
        }

        [TestCase(0, 1, 2)]
        [TestCase(0, 2, 1)]
        [TestCase(1, 2, 1)]
        [TestCase(0, 3, 0)]
        [TestCase(1, 3, 0)]
        [TestCase(2, 3, 0)]
        public void capacity_is_number_of_bytes_left_in_the_underlying_array(int start, int end, byte expectedCapacity)
        {
            var arr = new byte[] { 1, 2, 3 };
            var b = arr.Slice(start, end);
            Assert.AreEqual(expectedCapacity, b.Capacity);
        }

    }
}
