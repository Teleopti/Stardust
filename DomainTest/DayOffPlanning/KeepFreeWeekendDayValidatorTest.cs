using System;
using System.Collections;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
    [TestFixture]
    public class KeepFreeWeekendDayValidatorTest
    {
    	private KeepFreeWeekendDayValidator _target;
        private IOfficialWeekendDays _officialWeekendDays;
        private MinMax<int> _periodRange;
        private BitArray _periodDays;
        private BitArray _originalPeriodDays;

    	[SetUp]
        public void Setup()
        {
            _officialWeekendDays = new OfficialWeekendDays();
            _periodDays = createBitArrayForTest();
            _originalPeriodDays = createOriginalBitArrayForTest();
        }

        [Test]
        public void VerifyValidationIsTrueForTheSameArray()
        {
            _periodRange = new MinMax<int>(13, 19);

            _target = new KeepFreeWeekendDayValidator(_originalPeriodDays, _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(_originalPeriodDays, 13));
            Assert.IsTrue(_target.IsValid(_originalPeriodDays, 16));

            _periodRange = new MinMax<int>(12, 20);

            _target = new KeepFreeWeekendDayValidator(_originalPeriodDays, _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(_originalPeriodDays, 12));
            Assert.IsTrue(_target.IsValid(_originalPeriodDays, 13));
            Assert.IsTrue(_target.IsValid(_originalPeriodDays, 16));
            Assert.IsTrue(_target.IsValid(_originalPeriodDays, 20));
        }

        [Test]
        public void VerifyValidation()
        {
            _periodRange = new MinMax<int>(12, 19);
            _target = new KeepFreeWeekendDayValidator(_originalPeriodDays, _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(_periodDays, 13));

            _periodRange = new MinMax<int>(13, 19);
            _target = new KeepFreeWeekendDayValidator(_originalPeriodDays, _officialWeekendDays, _periodRange);
            Assert.IsFalse(_target.IsValid(_periodDays, 13));

            _periodRange = new MinMax<int>(13, 20);
            _target = new KeepFreeWeekendDayValidator(_originalPeriodDays, _officialWeekendDays, _periodRange);
            Assert.IsFalse(_target.IsValid(_periodDays, 13));

            _periodRange = new MinMax<int>(12, 20);
            _target = new KeepFreeWeekendDayValidator(_originalPeriodDays, _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(_periodDays, 13));
        }

    	[Test]
        public void VerifyFirstValidDayInPeriod()
        {
            _periodRange = new MinMax<int>(13, 19);

            _target = new KeepFreeWeekendDayValidator(_originalPeriodDays, _officialWeekendDays, _periodRange);
            Assert.IsFalse(_target.IsValid(_periodDays, 13));
        }

        [Test]
        public void VerifyLastValidDayInPeriod()
        {
            _periodRange = new MinMax<int>(13, 19);

            _target = new KeepFreeWeekendDayValidator(_originalPeriodDays, _officialWeekendDays, _periodRange);
            Assert.IsFalse(_target.IsValid(_periodDays, 19));
        }

        [Test]
        public void VerifyInvalidDayBeforeFirstDayInPeriod()
        {
            _periodRange = new MinMax<int>(13, 19);

            _target = new KeepFreeWeekendDayValidator(_originalPeriodDays, _officialWeekendDays, _periodRange);
	        Assert.Throws<ArgumentOutOfRangeException>(() => _target.IsValid(_periodDays, 12));
        }

        [Test]
        public void VerifyInvalidDayAfterLastDayInPeriod()
        {
            _periodRange = new MinMax<int>(13, 19);

            _target = new KeepFreeWeekendDayValidator(_originalPeriodDays, _officialWeekendDays, _periodRange);
			Assert.Throws<ArgumentOutOfRangeException>(() => _target.IsValid(_periodDays, 20));
		}


    	private static BitArray createOriginalBitArrayForTest()
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
                                    false,
                                    false,
                                    false,
                                    true,
                                    true,

                                    false,
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
                                    true,
                                    true
                                };
            BitArray result = new BitArray(values);
            return result;
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
                                    false,
                                    false,
                                    false,
                                    false, // true in original
                                    true,

                                    false,
                                    false,
                                    false, // true in original
                                    false,
                                    false,
                                    true, // false in original
                                    true,

                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    true,
                                    true
                                };
            BitArray result = new BitArray(values);
            return result;
        }
    }
}