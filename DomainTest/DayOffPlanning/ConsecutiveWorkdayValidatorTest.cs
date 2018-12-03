using System;
using System.Collections;
using NUnit.Framework;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
    [TestFixture]
    public class ConsecutiveWorkdayValidatorTest
    {

        #region Variables

        private ConsecutiveWorkdayValidator _target;
        private MinMax<int> _options;
        private BitArray _periodDays;

        #endregion

        [SetUp]
        public void Setup()
        {
            _options = new MinMax<int>(1, 6);
            _periodDays = createBitArrayForTest();

        }

        [Test]
        public void VerifyCheckForMinimumNeedForFirstDayOffWhenConsiderWeekBefore()
        {
            _options = new MinMax<int>(2, 44);
            bool[] values = new bool[]
                                {
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    true,

                                    false,
                                    true,
                                    false,
                                    false,
                                    false,
                                    true,
                                    false,

                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false
                                };
            BitArray bitArray = new BitArray(values);
            _target = new ConsecutiveWorkdayValidator(_options, true, false);
            bool result = _target.IsValid(bitArray, 8);
            Assert.IsFalse(result);
        }

        [Test]
        public void VerifyCheckForMinimumNoNeedForFirstDayOffWhenNotConsiderWeekBefore()
        {
            _options = new MinMax<int>(2, 4);
            bool[] values = new bool[]
                                {
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    true,

                                    false,
                                    true,
                                    false,
                                    false,
                                    false,
                                    true,
                                    false,

                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false
                                };
            BitArray bitArray = new BitArray(values);
            _target = new ConsecutiveWorkdayValidator(_options, false, false);
            bool result = _target.IsValid(bitArray, 8);
            Assert.IsTrue(result);
        }

        [Test]
        public void VerifyCheckForMaximumNeedForFirstDayOffWhenNotConsiderWeekBefore()
        {
            _options = new MinMax<int>(2, 3);
            bool[] values = new bool[]
                                {
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,

                                    false,
                                    false,
                                    false,
                                    false,
                                    true,
                                    true,
                                    false,

                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false
                                };
            BitArray bitArray = new BitArray(values);
            _target = new ConsecutiveWorkdayValidator(_options, false, false);
            bool result = _target.IsValid(bitArray, 11);
            Assert.IsFalse(result);
            result = _target.IsValid(bitArray, 12);
            Assert.IsTrue(result);
        }

        [Test]
        public void VerifyCheckForMinimumNeedForLastDayOffWhenConsiderWeekAfter()
        {
            _options = new MinMax<int>(2, 44);
            bool[] values = new bool[]
                                {
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    true,

                                    false,
                                    true,
                                    false,
                                    false,
                                    false,
                                    true,
                                    false,

                                    true,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false
                                };
            BitArray bitArray = new BitArray(values);
            _target = new ConsecutiveWorkdayValidator(_options, false, true);
            bool result = _target.IsValid(bitArray, 12);
            Assert.IsFalse(result);
        }

        [Test]
        public void VerifyCheckForMaximumNoNeedForLastDayOffWhenNotConsiderWeekAfter()
        {
            _options = new MinMax<int>(2, 4);
            bool[] values = new bool[]
                                {
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,

                                    false,
                                    true,
                                    false,
                                    false,
                                    false,
                                    true,
                                    false,

                                    true,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false
                                };
            BitArray bitArray = new BitArray(values);
            _target = new ConsecutiveWorkdayValidator(_options, false, false);
            bool result = _target.IsValid(bitArray, 12);
            Assert.IsTrue(result);
        }

        [Test]
        public void VerifyCheckForMaximumNeedForLastDayOffWhenNotConsiderWeekAfter()
        {
            _options = new MinMax<int>(2, 3);
            bool[] values = new bool[]
                                {
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,

                                    false,
                                    true,
                                    true,
                                    false,
                                    false,
                                    false,
                                    false,

                                    false,
                                    true,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false
                                };
            BitArray bitArray = new BitArray(values);
            _target = new ConsecutiveWorkdayValidator(_options, false, false);
            bool result = _target.IsValid(bitArray, 9);
            Assert.IsFalse(result);
        }

        [Test]
        public void VerifyValidationTooFewWorkdaysValidForTwoConsecutiveDayOffs()
        {
            _target = new ConsecutiveWorkdayValidator(new MinMax<int>(1, 7), true, true);

            bool result = _target.IsValid(_periodDays, 25);
            Assert.IsTrue(result);

        }

        [Test]
        public void VerifyValidationTooFewWorkdaysValidStartDay()
        {
            _target = new ConsecutiveWorkdayValidator(new MinMax<int>(1, 6), true, true);

            bool result = _target.IsValid(_periodDays, 0);
            Assert.IsTrue(result);
        }

        [Test]
        public void VerifyValidationConsiderWeekBefore()
        {
            _target = new ConsecutiveWorkdayValidator(new MinMax<int>(1, 7), true, false);
            bool result = _target.IsValid(_periodDays, 9);
            Assert.IsTrue(result);

            _target = new ConsecutiveWorkdayValidator(new MinMax<int>(1, 6), true, false);
            result = _target.IsValid(_periodDays, 9);
            Assert.IsFalse(result);

            _target = new ConsecutiveWorkdayValidator(new MinMax<int>(1, 6), false, false);

            result = _target.IsValid(_periodDays, 9);

            Assert.IsTrue(result);

        }

        [Test]
        public void VerifyValidationConsiderWeekAfterWhenLastInnerDayIsWorkday()
        {
            _target = new ConsecutiveWorkdayValidator(new MinMax<int>(1, 5), false, false);
            bool result = _target.IsValid(_periodDays, 26);
            Assert.IsTrue(result);

            _target = new ConsecutiveWorkdayValidator(new MinMax<int>(1, 5), false, true);
            result = _target.IsValid(_periodDays, 26);
            Assert.IsFalse(result);

        }

        [Test]
        public void VerifyValidationConsiderWeekAfterWhenLastInnerDayIsDayOff()
        {
            _periodDays = createBitArrayWhenLastInnerDayIsDayOff();

            _target = new ConsecutiveWorkdayValidator(new MinMax<int>(1, 4), false, false);
            bool result = _target.IsValid(_periodDays, 27);
            Assert.IsTrue(result);

            _target = new ConsecutiveWorkdayValidator(new MinMax<int>(1, 4), false, true);
            result = _target.IsValid(_periodDays, 27);
            Assert.IsFalse(result);
        }

        [Test]
        public void VerifyValidationTooManyWorkdays()
        {
            _target = new ConsecutiveWorkdayValidator(_options, true, true);

            bool result = _target.IsValid(_periodDays, 25);

            Assert.IsFalse(result);

            result = _target.IsValid(_periodDays, 17);

            Assert.IsFalse(result);

            _target = new ConsecutiveWorkdayValidator(new MinMax<int>(1, 8), true, true);

            result = _target.IsValid(_periodDays, 25);

            Assert.IsTrue(result);

            result = _target.IsValid(_periodDays, 17);

            Assert.IsTrue(result);

        }

        [Test]
        public void VerifyValidationTooFewWorkdays()
        {
            _target = new ConsecutiveWorkdayValidator(new MinMax<int>(1, 35), true, true);

            bool result = _target.IsValid(_periodDays, 9);
            Assert.True(result);

            result = _target.IsValid(_periodDays, 11);
            Assert.IsTrue(result);

            _target = new ConsecutiveWorkdayValidator(new MinMax<int>(2, 35), true, true);

            result = _target.IsValid(_periodDays, 9);
            Assert.IsFalse(result);

            result = _target.IsValid(_periodDays, 11);
            Assert.IsFalse(result);

        }

        [Test]
        public void VerifyValidationThrowsExceptionIfNoValidDate()
        {
            _target = new ConsecutiveWorkdayValidator(new MinMax<int>(1, 7), true, true);
	        Assert.Throws<ArgumentOutOfRangeException>(() => _target.IsValid(_periodDays, 35));
        }

        [Test]
        public void VerifyIsValid()
        {
            BitArray bitArray = createBitArrayForTest();
            _target = new ConsecutiveWorkdayValidator(new MinMax<int>(2, 6), true, true);
            Assert.IsTrue(_target.IsValid(bitArray, 0));
            Assert.IsFalse(_target.IsValid(bitArray, 1));
            Assert.IsFalse(_target.IsValid(bitArray, 9));
            Assert.IsFalse(_target.IsValid(bitArray, 34));

            _target = new ConsecutiveWorkdayValidator(new MinMax<int>(1, 3), true, true);
            Assert.IsFalse(_target.IsValid(bitArray, 9));
            Assert.IsFalse(_target.IsValid(bitArray, 26));

            _target = new ConsecutiveWorkdayValidator(new MinMax<int>(1, 3), false, false);
            Assert.IsTrue(_target.IsValid(bitArray, 9));
            Assert.IsTrue(_target.IsValid(bitArray, 26));
        }

        [Test]
        public void VerifyIsValidThrowsExceptionWhenNotIndexIsNotDayOff()
        {
            BitArray bitArray = createBitArrayForTest();
            _target = new ConsecutiveWorkdayValidator(new MinMax<int>(1, 3), true, true);
			Assert.Throws<ArgumentException>(() => _target.IsValid(bitArray, 2));
		}

        private static BitArray createBitArrayForTest()
        {
            bool[] values = new bool[]
                                {
                                    true,
                                    true,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,

                                    false,
                                    false,
                                    true,
                                    false,
                                    true,
                                    true,
                                    false,

                                    false,
                                    false,
                                    false,
                                    true,
                                    false,
                                    false,
                                    false,

                                    false,
                                    false,
                                    false,
                                    false,
                                    true,
                                    true,
                                    false,

                                    false,
                                    false, 
                                    false,
                                    false,
                                    false,
                                    false,
                                    true
                                };
            BitArray result = new BitArray(values);
            return result;
        }

        private static BitArray createBitArrayWhenLastInnerDayIsDayOff()
        {
            bool[] values = new bool[]
                                {
                                    true,
                                    true,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,

                                    false,
                                    false,
                                    true,
                                    false,
                                    true,
                                    true,
                                    false,

                                    false,
                                    false,
                                    false,
                                    true,
                                    false,
                                    false,
                                    false,

                                    false,
                                    false,
                                    false,
                                    false,
                                    true,
                                    true,
                                    true,

                                    false,
                                    false, 
                                    false,
                                    false,
                                    false,
                                    false,
                                    true
                                };
            BitArray result = new BitArray(values);
            return result;
        }

		[Test]
		public void ShouldNotConsiderWeekBeforeIfEmpty()
		{
			_options = new MinMax<int>(2, 6);
			bool[] values = new bool[]
                                {
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,

                                    false,
                                    false,
                                    false,
                                    false,
                                    true,
                                    true,
                                    false,

                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false
                                };
			BitArray bitArray = new BitArray(values);
			_target = new ConsecutiveWorkdayValidator(_options, true, false);
			bool result = _target.IsValid(bitArray, 11);
			Assert.IsTrue(result);
			result = _target.IsValid(bitArray, 12);
			Assert.IsTrue(result);
		}

		[Test]
		public void ShouldNotConsiderWeekAfterIfEmpty()
		{
			_options = new MinMax<int>(2, 6);
			bool[] values = new bool[]
                                {
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,

                                    false,
                                    false,
                                    false,
                                    false,
                                    true,
                                    true,
                                    false,

                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false
                                };
			BitArray bitArray = new BitArray(values);
			_target = new ConsecutiveWorkdayValidator(_options, false, true);
			bool result = _target.IsValid(bitArray, 11);
			Assert.IsTrue(result);
			result = _target.IsValid(bitArray, 12);
			Assert.IsTrue(result);
		}
    }
}