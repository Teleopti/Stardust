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
    public class FreeWeekendDayValidatorTest
    {
    	private FreeWeekendDayValidator _target;
        private IOfficialWeekendDays _officialWeekendDays;
        private MinMax<int> _periodRange;
        private BitArray _periodDays;
    	
    	[SetUp]
        public void Setup()
        {
            _officialWeekendDays = new OfficialWeekendDays();
            _periodDays = createBitArrayForTest();
        }

        [Test]
        public void VerifyValidation()
        {
            _periodRange = new MinMax<int>(13, 19);

            _target = new FreeWeekendDayValidator(new MinMax<int>(1, 1), _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(_periodDays, 13));

            _target = new FreeWeekendDayValidator(new MinMax<int>(2, 2), _officialWeekendDays, _periodRange);
            Assert.IsFalse(_target.IsValid(_periodDays, 13));

            _periodRange = new MinMax<int>(12, 19);

            _target = new FreeWeekendDayValidator(new MinMax<int>(2, 2), _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(_periodDays, 13));

            _target = new FreeWeekendDayValidator(new MinMax<int>(3, 3), _officialWeekendDays, _periodRange);
            Assert.IsFalse(_target.IsValid(_periodDays, 13));

        }

        [Test]
        public void VerifyIsValidWithBitArray()
        {
            BitArray bitArray = createBitArrayForTest();

            _periodRange = new MinMax<int>(13, 19);

            _target = new FreeWeekendDayValidator(new MinMax<int>(1, 1), _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(bitArray, 13));

            _target = new FreeWeekendDayValidator(new MinMax<int>(2, 2), _officialWeekendDays, _periodRange);
            Assert.IsFalse(_target.IsValid(bitArray, 13));

            _periodRange = new MinMax<int>(12, 19);
            _target = new FreeWeekendDayValidator(new MinMax<int>(2, 2), _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(bitArray, 13));

            _target = new FreeWeekendDayValidator(new MinMax<int>(3, 3), _officialWeekendDays, _periodRange);
            Assert.IsFalse(_target.IsValid(bitArray, 13));
        }

    	[Test]
        public void VerifyFirstValidDayInPeriod()
        {
            _periodRange = new MinMax<int>(13, 19);

            _target = new FreeWeekendDayValidator(new MinMax<int>(1, 7), _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(_periodDays, 13));
        }

        [Test]
        public void VerifyLastValidDayInPeriod()
        {
            _periodRange = new MinMax<int>(13, 19);

            _target = new FreeWeekendDayValidator(new MinMax<int>(1, 7), _officialWeekendDays, _periodRange);
	        Assert.Throws<ArgumentException>(() => _target.IsValid(_periodDays, 19));
        }

        [Test]
        public void VerifyInvalidDayBeforeFirstDayInPeriod()
        {
            _periodRange = new MinMax<int>(13, 19);

            _target = new FreeWeekendDayValidator(new MinMax<int>(1, 7), _officialWeekendDays, _periodRange);
			Assert.Throws<ArgumentOutOfRangeException>(() => _target.IsValid(_periodDays, 12));
		}

        [Test]
        public void VerifyInvalidDayAfterLastDayInPeriod()
        {
            _periodRange = new MinMax<int>(13, 19);

            _target = new FreeWeekendDayValidator(new MinMax<int>(1, 7), _officialWeekendDays, _periodRange);
			Assert.Throws<ArgumentOutOfRangeException>(() => _target.IsValid(_periodDays, 20));
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
    }
}