using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest
{
    [TestFixture]
    public class BinaryPermutationTest
    {
        private BinaryPermutation _binaryPermutation;
        private BitArray _array;

        [SetUp]
        public void Setup()
        {
            _array = new BitArray(4, false);
        }

        [Test]
        public void VerifyAllPermutationWithAllFalseValueArray()
        {
            _binaryPermutation = new BinaryPermutation(_array, new AlwaysTrueValidator());
            IList<BitArray> result = _binaryPermutation.IterateAll();
            Assert.AreEqual(16, result.Count);
            Assert.AreEqual("0000", BitArrayHelper.ToString(result[0]));
            Assert.AreEqual("1000", BitArrayHelper.ToString(result[1]));
            Assert.AreEqual("0100", BitArrayHelper.ToString(result[2]));
            Assert.AreEqual("1100", BitArrayHelper.ToString(result[3]));
            Assert.AreEqual("0010", BitArrayHelper.ToString(result[4]));
            Assert.AreEqual("1010", BitArrayHelper.ToString(result[5]));
            Assert.AreEqual("0110", BitArrayHelper.ToString(result[6]));
            Assert.AreEqual("1110", BitArrayHelper.ToString(result[7]));
            Assert.AreEqual("0001", BitArrayHelper.ToString(result[8]));
            Assert.AreEqual("1001", BitArrayHelper.ToString(result[9]));
            Assert.AreEqual("0101", BitArrayHelper.ToString(result[10]));
            Assert.AreEqual("1101", BitArrayHelper.ToString(result[11]));
            Assert.AreEqual("0011", BitArrayHelper.ToString(result[12]));
            Assert.AreEqual("1011", BitArrayHelper.ToString(result[13]));
            Assert.AreEqual("0111", BitArrayHelper.ToString(result[14]));
            Assert.AreEqual("1111", BitArrayHelper.ToString(result[15]));
        }

        [Test]
        public void VerifyAllPermutationWithAllFalseArrayAndStartEndIndex()
        {
            int startIndex = 0;
            int endIndex = _array.Count - 1;

            _binaryPermutation = new BinaryPermutation(_array, new List<IBinaryValidator> { new AlwaysTrueValidator() }, startIndex, endIndex);
            IList<BitArray> result = _binaryPermutation.IterateAll();
            Assert.AreEqual(16, result.Count);
            Assert.AreEqual("0000", BitArrayHelper.ToString(result[0]));
            Assert.AreEqual("1000", BitArrayHelper.ToString(result[1]));
            Assert.AreEqual("0100", BitArrayHelper.ToString(result[2]));
            Assert.AreEqual("1100", BitArrayHelper.ToString(result[3]));
            Assert.AreEqual("0010", BitArrayHelper.ToString(result[4]));
            Assert.AreEqual("1010", BitArrayHelper.ToString(result[5]));
            Assert.AreEqual("0110", BitArrayHelper.ToString(result[6]));
            Assert.AreEqual("1110", BitArrayHelper.ToString(result[7]));
            Assert.AreEqual("0001", BitArrayHelper.ToString(result[8]));
            Assert.AreEqual("1001", BitArrayHelper.ToString(result[9]));
            Assert.AreEqual("0101", BitArrayHelper.ToString(result[10]));
            Assert.AreEqual("1101", BitArrayHelper.ToString(result[11]));
            Assert.AreEqual("0011", BitArrayHelper.ToString(result[12]));
            Assert.AreEqual("1011", BitArrayHelper.ToString(result[13]));
            Assert.AreEqual("0111", BitArrayHelper.ToString(result[14]));
            Assert.AreEqual("1111", BitArrayHelper.ToString(result[15]));

            startIndex = 0;
            endIndex = 2;
            _binaryPermutation = new BinaryPermutation(_array, new List<IBinaryValidator> { new AlwaysTrueValidator() }, startIndex, endIndex);
            result = _binaryPermutation.IterateAll();
            Assert.AreEqual(8, result.Count);
            Assert.AreEqual("0000", BitArrayHelper.ToString(result[0]));
            Assert.AreEqual("1000", BitArrayHelper.ToString(result[1]));
            Assert.AreEqual("0100", BitArrayHelper.ToString(result[2]));
            Assert.AreEqual("1100", BitArrayHelper.ToString(result[3]));
            Assert.AreEqual("0010", BitArrayHelper.ToString(result[4]));
            Assert.AreEqual("1010", BitArrayHelper.ToString(result[5]));
            Assert.AreEqual("0110", BitArrayHelper.ToString(result[6]));
            Assert.AreEqual("1110", BitArrayHelper.ToString(result[7]));

            startIndex = 1;
            endIndex = 3;
            _binaryPermutation = new BinaryPermutation(_array, new List<IBinaryValidator> { new AlwaysTrueValidator() }, startIndex, endIndex);
            result = _binaryPermutation.IterateAll();
            Assert.AreEqual(8, result.Count);
            Assert.AreEqual("0000", BitArrayHelper.ToString(result[0]));
            Assert.AreEqual("0100", BitArrayHelper.ToString(result[1]));
            Assert.AreEqual("0010", BitArrayHelper.ToString(result[2]));
            Assert.AreEqual("0110", BitArrayHelper.ToString(result[3]));
            Assert.AreEqual("0001", BitArrayHelper.ToString(result[4]));
            Assert.AreEqual("0101", BitArrayHelper.ToString(result[5]));
            Assert.AreEqual("0011", BitArrayHelper.ToString(result[6]));
            Assert.AreEqual("0111", BitArrayHelper.ToString(result[7]));

            startIndex = 1;
            endIndex = 2;
            _binaryPermutation = new BinaryPermutation(_array, new List<IBinaryValidator> { new AlwaysTrueValidator() }, startIndex, endIndex);
            result = _binaryPermutation.IterateAll();
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual("0000", BitArrayHelper.ToString(result[0]));
            Assert.AreEqual("0100", BitArrayHelper.ToString(result[1]));
            Assert.AreEqual("0010", BitArrayHelper.ToString(result[2]));
            Assert.AreEqual("0110", BitArrayHelper.ToString(result[3]));
        }

        [Test]
        public void VerifyAllPermutationStepsThroughAllCases()
        {
            _binaryPermutation = new BinaryPermutation(_array, new AlwaysTrueValidator());
            IList<BitArray> resultList = _binaryPermutation.IterateAll();
            int resultWithEmptyArray = resultList.Count;

            SetRangeFromLeft(_array, 2);
            _binaryPermutation = new BinaryPermutation(_array, new AlwaysTrueValidator());
            resultList = _binaryPermutation.IterateAll();
            Assert.AreEqual(resultWithEmptyArray, resultList.Count);

            SetRangeFromLeft(_array, 3);
            _binaryPermutation = new BinaryPermutation(_array, new AlwaysTrueValidator());
            resultList = _binaryPermutation.IterateAll();
            Assert.AreEqual(resultWithEmptyArray, resultList.Count);
        }

        [Test]
        public void VerifyFindFirstPermutationReturnsTheInputIfValidAndReturnAsClone()
        {
            _binaryPermutation = new BinaryPermutation(_array, new AlwaysTrueValidator());
            
            // as every state is valid, the input is valid
            BitArray result = _binaryPermutation.FindFirstValid();

            Assert.AreNotSame(_array, result);
            Assert.AreEqual(BitArrayHelper.ToString(_array), BitArrayHelper.ToString(result));
        }

        [Test]
        public void VerifyFindFirstPermutationReallyFindTheFirstValidAndReturnAsClone()
        {
            _binaryPermutation = new BinaryPermutation(_array, new ArrayValueValidator(2));

            // as every state is valid, the input is valid
            BitArray result = _binaryPermutation.FindFirstValid();

            Assert.AreNotSame(_array, result);
            Assert.AreEqual(BitArrayHelper.ToString(_array), BitArrayHelper.ToString(result));
        }

        [Test]
        public void VerifyFindFirstPermutationReturnNullIfNoValid()
        {
            _binaryPermutation = new BinaryPermutation(_array, new AlwaysFalseValidator());

            BitArray result = _binaryPermutation.FindFirstValid();
            Assert.IsNull(result);
        }

        [Test]
        public void VerifyPermutationWithArrayValueValidator()
        {
            _binaryPermutation = new BinaryPermutation(_array, new ArrayValueValidator(2));
            SetRangeFromLeft(_array, 2);
            IList<BitArray> result = _binaryPermutation.IterateAll();
            Assert.AreEqual(6, result.Count);
            Assert.AreEqual("1100", BitArrayHelper.ToString(result[0]));
            Assert.AreEqual("1010", BitArrayHelper.ToString(result[1]));
            Assert.AreEqual("0110", BitArrayHelper.ToString(result[2]));
            Assert.AreEqual("1001", BitArrayHelper.ToString(result[3]));
            Assert.AreEqual("0101", BitArrayHelper.ToString(result[4]));
            Assert.AreEqual("0011", BitArrayHelper.ToString(result[5]));

            _binaryPermutation = new BinaryPermutation(_array, new ArrayValueValidator(_array));
            SetRangeFromLeft(_array, 2);
            result = _binaryPermutation.IterateAll();
            Assert.AreEqual(6, result.Count);
            Assert.AreEqual("1100", BitArrayHelper.ToString(result[0]));
            Assert.AreEqual("1010", BitArrayHelper.ToString(result[1]));
            Assert.AreEqual("0110", BitArrayHelper.ToString(result[2]));
            Assert.AreEqual("1001", BitArrayHelper.ToString(result[3]));
            Assert.AreEqual("0101", BitArrayHelper.ToString(result[4]));
            Assert.AreEqual("0011", BitArrayHelper.ToString(result[5]));
        }

        [Test]
        public void VerifyPermutationWithLockedValueValidator()
        {
            _array.Set(1, true);
            BitArray locks = new BitArray(4, false);
            BitArray original = (BitArray)_array.Clone();
            locks.Set(1, true); // lockes the 2-nd
            locks.Set(2, true); // lockes the 3-rd
            _binaryPermutation = new BinaryPermutation(_array, new LockedValueValidator(original, locks));
            IList<BitArray> result = _binaryPermutation.IterateAll();
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual("0100", BitArrayHelper.ToString(result[0]));
            Assert.AreEqual("1100", BitArrayHelper.ToString(result[1]));
            Assert.AreEqual("0101", BitArrayHelper.ToString(result[2]));
            Assert.AreEqual("1101", BitArrayHelper.ToString(result[3]));
        }

        [Test]
        public void VerifyPermutationWithMultipleValidator()
        {
            _array.Set(1, true);
            BitArray locks = new BitArray(4, false);
            BitArray original = (BitArray)_array.Clone();
            locks.Set(1, true); // lockes the 2-nd
            locks.Set(2, true); // lockes the 3-rd
            _binaryPermutation = new BinaryPermutation(
                _array,
                new List<IBinaryValidator> { new ArrayValueValidator(2), new LockedValueValidator(original, locks) });
            // the third day is locked, it musb be 1 there
            IList<BitArray> result = _binaryPermutation.IterateAll();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("1100", BitArrayHelper.ToString(result[0]));
            Assert.AreEqual("0101", BitArrayHelper.ToString(result[1]));
        }

        [Test]
        [Ignore("Takes much time")]
        public void VerifyPermutationWithManyMember()
        {
            _array = new BitArray(56, false);
            SetRangeFromLeft(_array, 16);
            _binaryPermutation = new BinaryPermutation(_array, new AlwaysTrueValidator());
            IList<BitArray> result = _binaryPermutation.IterateAll(); 
            Assert.AreEqual(6, result.Count);
        }

        #region Local methods


        //private void SetRangeFromRight(BitArray array, int number)
        //{
        //    for (int i = array.Count - 1; i >= array.Count-number; i--)
        //    {
        //        array.Set(i, true);
        //    }
        //}

        private static void SetRangeFromLeft(BitArray array, int number)
        {
            for (int i = 0; i < number; i++)
            {
                array.Set(i, true);
            }
        }

        #endregion

    }
}