using System;
using System.Collections;
using NUnit.Framework;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
    [TestFixture]
    public class ConsecutiveDayOffValidatorTest
    {

        #region Variables

        private ConsecutiveDayOffValidator _target;
        private BitArray _periodDays;


        #endregion

        [SetUp]
        public void Setup()
        {
            _periodDays = createBitArrayForTest();
        }

        [Test]
        public void VerifyValidationTooManyDayOffs()
        {
            _target = new ConsecutiveDayOffValidator(new MinMax<int>(1, 3), true, true);
            bool result = _target.IsValid(_periodDays, 18);
            Assert.IsFalse(result);

            _target = new ConsecutiveDayOffValidator( new MinMax<int>(1, 4), true, true);
            result = _target.IsValid(_periodDays, 18);
            Assert.IsTrue(result);
        }

        [Test]
        public void VerifyValidationTooFewDayOffs()
        {
            _target = new ConsecutiveDayOffValidator(new MinMax<int>(2, 4), true, true);
            Assert.IsFalse(_target.IsValid(_periodDays, 13));

            _target = new ConsecutiveDayOffValidator(new MinMax<int>(1, 4), true, true);
            Assert.IsTrue(_target.IsValid(_periodDays, 13));

        }

        [Test]
        public void VerifyValidationTooFewDayOffsValidForTheWholeDayOffBlock()
        {

            _target = new ConsecutiveDayOffValidator(new MinMax<int>(1, 3), true, true);
            Assert.IsFalse(_target.IsValid(_periodDays, 17));
            Assert.IsFalse(_target.IsValid(_periodDays, 18));
            Assert.IsFalse(_target.IsValid(_periodDays, 19));
            Assert.IsFalse(_target.IsValid(_periodDays, 20));

            _target = new ConsecutiveDayOffValidator(new MinMax<int>(1, 4), true, true);
            Assert.IsTrue(_target.IsValid(_periodDays, 17));
            Assert.IsTrue(_target.IsValid(_periodDays, 18));
            Assert.IsTrue(_target.IsValid(_periodDays, 19));
            Assert.IsTrue(_target.IsValid(_periodDays, 20));

        }

        #region Week before and after

        [Test]
        public void VerifyValidationStartAndEndDaysAreCorrect()
        {

            _target = new ConsecutiveDayOffValidator(new MinMax<int>(1, 6), true, true);
            Assert.IsTrue(_target.IsValid(_periodDays, 0));
            Assert.IsTrue(_target.IsValid(_periodDays, 34));

            _target = new ConsecutiveDayOffValidator(new MinMax<int>(2, 6), true, true);
            Assert.IsFalse(_target.IsValid(_periodDays, 0));
            Assert.IsFalse(_target.IsValid(_periodDays, 34));
        }

        [Test]
        public void VerifyValidationConsiderWeekBefore()
        {

            _target = new ConsecutiveDayOffValidator(new MinMax<int>(1, 3), false, false);
            Assert.IsTrue(_target.IsValid(_periodDays, 7));

            _target = new ConsecutiveDayOffValidator(new MinMax<int>(1, 3), true, false);
            Assert.IsFalse(_target.IsValid(_periodDays, 7));
        }

        [Test]
        public void VerifyValidationConsiderWeekAfter()
        {
            _target = new ConsecutiveDayOffValidator(new MinMax<int>(1, 3), false, false);
            Assert.IsTrue(_target.IsValid(_periodDays, 27));

            _target = new ConsecutiveDayOffValidator(new MinMax<int>(1, 3), false, true);
            Assert.IsFalse(_target.IsValid(_periodDays, 27));
        }

        [Test]
        public void VerifyFirstAndLastValidDayInPeriodWithWeekBeforeAndAfter()
        {
            _target = new ConsecutiveDayOffValidator(new MinMax<int>(1, 7), true, true);
            Assert.IsTrue(_target.IsValid(_periodDays, 0));
            Assert.IsTrue(_target.IsValid(_periodDays, 6));

            _target = new ConsecutiveDayOffValidator(new MinMax<int>(1, 7), false, true);
            Assert.IsTrue(_target.IsValid(_periodDays, 7));
            Assert.IsTrue(_target.IsValid(_periodDays, 34));

            _target = new ConsecutiveDayOffValidator(new MinMax<int>(1, 7), true, false);
            Assert.IsTrue(_target.IsValid(_periodDays, 0));
            Assert.IsTrue(_target.IsValid(_periodDays, 27));

            _target = new ConsecutiveDayOffValidator(new MinMax<int>(1, 7), false, false);
            Assert.IsTrue(_target.IsValid(_periodDays, 7));
            Assert.IsTrue(_target.IsValid(_periodDays, 27));
        }

        [Test]
        public void VerifyInvalidDayBeforeFirstDayInPeriodWhenConsideringBothWeekBeforeAndWeekAfter()
        {
            _target = new ConsecutiveDayOffValidator(new MinMax<int>(1, 7), true, true);
	        Assert.Throws<ArgumentOutOfRangeException>(() => _target.IsValid(_periodDays, -1));
        }

        [Test]
        public void VerifyInvalidDayAfterLastDayInPeriodWhenConsideringBothWeekBeforeAndWeekAfter()
        {
            _target = new ConsecutiveDayOffValidator(new MinMax<int>(1, 7), true, true);
			Assert.Throws<ArgumentOutOfRangeException>(() => _target.IsValid(_periodDays, 35));
        }

        [Test]
        public void VerifyInvalidDayBeforeFirstDayInPeriodWhenConsideringWeekBefore()
        {
            _target = new ConsecutiveDayOffValidator(new MinMax<int>(1, 7), true, false);
			Assert.Throws<ArgumentOutOfRangeException>(() => _target.IsValid(_periodDays, 30));
		}

        [Test]
        public void VerifyInvalidDayAfterLastDayInPeriodWhenConsideringWeekBefore()
        {
            _target = new ConsecutiveDayOffValidator(new MinMax<int>(1, 7), true, false);
			Assert.Throws<ArgumentOutOfRangeException>(() => _target.IsValid(_periodDays, 28));
		}

        [Test]
        public void VerifyInvalidDayBeforeFirstDayInPeriodWhenConsideringWeekAfter()
        {
            _target = new ConsecutiveDayOffValidator(new MinMax<int>(1, 7), false, true);
			Assert.Throws<ArgumentOutOfRangeException>(() => _target.IsValid(_periodDays, 5));
		}

        [Test]
        public void VerifyInvalidDayAfterLastDayInPeriodWhenConsideringWeekAfter()
        {
            _target = new ConsecutiveDayOffValidator(new MinMax<int>(1, 7), false, true);
			Assert.Throws<ArgumentOutOfRangeException>(() => _target.IsValid(_periodDays, 35));
		}

        [Test]
        public void VerifyInvalidDayBeforeFirstDayInPeriodWhenConsideringNeitherWeekBeforeNorWeekAfter()
        {
            _target = new ConsecutiveDayOffValidator(new MinMax<int>(1, 7), false, false);
			Assert.Throws<ArgumentOutOfRangeException>(() => _target.IsValid(_periodDays, 5));
		}

        [Test]
        public void VerifyInvalidDayAfterLastDayInPeriodWhenConsideringNeitherWeekBeforeNorWeekAfter()
        {
            _target = new ConsecutiveDayOffValidator(new MinMax<int>(1, 7), false, false);
			Assert.Throws<ArgumentOutOfRangeException>(() => _target.IsValid(_periodDays, 28));
		}

        [Test]
        public void VerifyIsValidWithBitArray()
        {
            BitArray bitArray = createBitArrayForTest();
            _target = new ConsecutiveDayOffValidator(new MinMax<int>(1, 3), true, true);
            Assert.IsTrue(_target.IsValid(bitArray, 0));
            Assert.IsFalse(_target.IsValid(bitArray, 7));
            Assert.IsTrue(_target.IsValid(bitArray, 34));
            _target = new ConsecutiveDayOffValidator(new MinMax<int>(2, 3), true, true);
            Assert.IsFalse(_target.IsValid(bitArray, 0));
            Assert.IsFalse(_target.IsValid(bitArray, 13));
            Assert.IsFalse(_target.IsValid(bitArray, 34));

            _target = new ConsecutiveDayOffValidator(new MinMax<int>(1, 3), false, true);
            Assert.IsTrue(_target.IsValid(bitArray, 7));
        }

        [Test]
        public void VerifyIsValidWithBitArrayThrowsExceptionWhenNotIndexIsNotDayOff()
        {
            BitArray bitArray = createBitArrayForTest();
            _target = new ConsecutiveDayOffValidator(new MinMax<int>(1, 3), true, true);
			Assert.Throws<ArgumentException>(() => _target.IsValid(bitArray, 1));
		}

        #endregion

        private static BitArray createBitArrayForTest()
        {
            bool[] values = new bool[]
                                {
                                    true,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    true,

                                    true,
                                    true,
                                    true,
                                    false,
                                    false,
                                    false,
                                    true,

                                    false,
                                    false,
                                    false,
                                    true,
                                    true,
                                    true,
                                    true,

                                    false,
                                    false,
                                    false,
                                    false,
                                    true,
                                    true,
                                    true,

                                    true,
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
    }
}