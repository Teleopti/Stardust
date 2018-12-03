using System;
using System.Collections;
using NUnit.Framework;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
    [TestFixture]
    public class WeeklyDayOffValidatorTest
    {

        #region Variables

        private WeeklyDayOffValidator _target;
        private BitArray _periodDays; 

        #endregion

        [SetUp]
        public void Setup()
        {
            _periodDays = createBitArrayForTest();
        }

        [Test]
        public void VerifyWeekStartAndEndCorrectlyCalculated()
        {
            MinMax<int> options = new MinMax<int>(2, 3);
            bool[] values = new[]
                                {
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
                                    true,
                                    true,
                                    true,

                                    true,
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
                                    false,
                                    false
                                };
            _periodDays = new BitArray(values);
            _target = new WeeklyDayOffValidator(options);
            bool result = _target.IsValid(_periodDays, 13);
            Assert.IsTrue(result);
            result = _target.IsValid(_periodDays, 14);
            Assert.IsFalse(result);
        }

        [Test]
        public void VerifyTooFewDayOffs()
        {
            _target = new WeeklyDayOffValidator(new MinMax<int>(4, 7));
            Assert.IsTrue(_target.IsValid(_periodDays, 8));

            _target = new WeeklyDayOffValidator(new MinMax<int>(5, 7));
            Assert.IsFalse(_target.IsValid(_periodDays, 8));

            _target = new WeeklyDayOffValidator(new MinMax<int>(3, 7));
            Assert.IsTrue(_target.IsValid(_periodDays, 18));

            _target = new WeeklyDayOffValidator(new MinMax<int>(4, 7));
            Assert.IsFalse(_target.IsValid(_periodDays, 18));

            _target = new WeeklyDayOffValidator(new MinMax<int>(1, 7));
            Assert.IsTrue(_target.IsValid(_periodDays, 27));

            _target = new WeeklyDayOffValidator(new MinMax<int>(2, 7));
            Assert.IsFalse(_target.IsValid(_periodDays, 27));


        }

        [Test]
        public void VerifyTooManyDayOffs()
        {
            _target = new WeeklyDayOffValidator(new MinMax<int>(1, 4));
            Assert.IsTrue(_target.IsValid(_periodDays, 8));

            _target = new WeeklyDayOffValidator(new MinMax<int>(1, 3));
            Assert.IsFalse(_target.IsValid(_periodDays, 8));

            _target = new WeeklyDayOffValidator(new MinMax<int>(1, 3));
            Assert.IsTrue(_target.IsValid(_periodDays, 18));

            _target = new WeeklyDayOffValidator(new MinMax<int>(1, 2));
            Assert.IsFalse(_target.IsValid(_periodDays, 18));

            _target = new WeeklyDayOffValidator(new MinMax<int>(1, 1));
            Assert.IsTrue(_target.IsValid(_periodDays, 27));
        }

        [Test]
        public void VerifyFirstAndLastValidDayInPeriod()
        {
            _target = new WeeklyDayOffValidator(new MinMax<int>(1, 7));
            Assert.IsTrue(_target.IsValid(_periodDays, 7));
            Assert.IsTrue(_target.IsValid(_periodDays, 27));
        }

        [Test]
        public void VerifyInvalidDayBeforeFirstDayInPeriod()
        {
            _target = new WeeklyDayOffValidator(new MinMax<int>(1, 7));
            Assert.Throws<ArgumentOutOfRangeException>(() => _target.IsValid(_periodDays, 6));
        }

        [Test]
        public void VerifyInvalidDayAfterLastDayInPeriod()
        {
            _target = new WeeklyDayOffValidator(new MinMax<int>(1, 7));
            Assert.Throws<ArgumentOutOfRangeException>(() => _target.IsValid(_periodDays, 31));
        }

        [Test]
        public void VerifyIsValidWithBitArray()
        {
            BitArray bitArray = createBitArrayForTest();
            _target = new WeeklyDayOffValidator(new MinMax<int>(1, 3));
            Assert.IsFalse(_target.IsValid(bitArray, 7));
            Assert.IsFalse(_target.IsValid(bitArray, 8));
            Assert.IsFalse(_target.IsValid(bitArray, 13));
            Assert.IsTrue(_target.IsValid(bitArray, 18));
            Assert.IsTrue(_target.IsValid(bitArray, 27));
        }

        [Test]
        public void VerifyIsValidWithBitArrayThrowsExceptionWhenIndexIsNotDayOff()
        {
            BitArray bitArray = createBitArrayForTest();
            _target = new WeeklyDayOffValidator(new MinMax<int>(1, 3));
            Assert.Throws<ArgumentException>(() => _target.IsValid(bitArray, 10));
        }

        private static BitArray createBitArrayForTest()
        {
            bool[] values = new bool[]
                                {
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
                                    true,

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
                                    true,

                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false
                                };
            BitArray result = new BitArray(values);
            return result;
        }

    }
}