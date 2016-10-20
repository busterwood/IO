using BusterWood.InputOutput;
using NUnit.Framework;

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

        [TestCase(0, 0, 0)] // zero length block is allowed
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

        [TestCase(0, 0, 3)] // zero length slice is allowed
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

        [Test]
        public void can_append_to_an_existing_array_that_has_spare_capacity()
        {
            var arr = new byte[] { 1, 0, 0 };
            var b1 = arr.Slice(0, 1);
            var b2 = b1.Append((byte)2);
            Assert.AreEqual(2, b2.Length);
            Assert.AreEqual(2, b2[1]);
            Assert.AreSame(arr, b2.Array);
        }

        [Test]
        public void appending_to_array_without_spare_capacity_creates_a_new_array()
        {
            var arr = new byte[] { 1 };
            var b1 = arr.Slice(0);
            var b2 = b1.Append((byte)2);
            Assert.AreEqual(2, b2.Length);
            Assert.AreEqual(2, b2[1]);
            Assert.AreNotSame(arr, b2.Array);
            Assert.AreEqual(4, b2.Capacity);
        }
    }
}
