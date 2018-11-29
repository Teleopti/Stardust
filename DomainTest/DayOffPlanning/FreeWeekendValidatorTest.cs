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
    public class FreeWeekendValidatorSwedenTest
    {
    	private FreeWeekendValidator _target;
        private MinMax<int> _periodRange;
        private BitArray _bitArray;
        private IOfficialWeekendDays _officialWeekendDays;

    	[SetUp]
        public void Setup()
        {
            _officialWeekendDays = new OfficialWeekendDays();
            _bitArray = createBitArrayForTest();
        }

        [Test]
        public void VerifyIsValidWithBitArray()
        {            
            _periodRange = new MinMax<int>(13, 19);

            _target = new FreeWeekendValidator(new MinMax<int>(0, 0), _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(_bitArray, 13));

            _target = new FreeWeekendValidator(new MinMax<int>(1, 1), _officialWeekendDays, _periodRange);
            Assert.IsFalse(_target.IsValid(_bitArray, 13));

            _periodRange = new MinMax<int>(12, 19);

            _target = new FreeWeekendValidator(new MinMax<int>(1, 1), _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(_bitArray, 13));

            _target = new FreeWeekendValidator(new MinMax<int>(2, 2), _officialWeekendDays, _periodRange);
            Assert.IsFalse(_target.IsValid(_bitArray, 13));
        }

    	[Test]
        public void VerifyFirstValidDayInPeriod()
        {
            _periodRange = new MinMax<int>(12, 19);
            _target = new FreeWeekendValidator(new MinMax<int>(2, 2), _officialWeekendDays, _periodRange);
            Assert.IsFalse(_target.IsValid(_bitArray, 13));
        }

        [Test]
        public void VerifyLastValidDayInPeriod()
        {
            _periodRange = new MinMax<int>(12, 19);
            _target = new FreeWeekendValidator(new MinMax<int>(2, 2), _officialWeekendDays, _periodRange);
			Assert.Throws<ArgumentException>(() => _target.IsValid(_bitArray, 19));
        }

        [Test]
        public void VerifyInvalidDayBeforeFirstDayInPeriod()
        {
            _periodRange = new MinMax<int>(12, 19);
            _target = new FreeWeekendValidator(new MinMax<int>(1, 7), _officialWeekendDays, _periodRange);
			Assert.Throws<ArgumentOutOfRangeException>(() => _target.IsValid(_bitArray, 11));
		}

        [Test]
        public void VerifyInvalidDayAfterLastDayInPeriod()
        {
            _periodRange = new MinMax<int>(12, 19);
            _target = new FreeWeekendValidator(new MinMax<int>(1, 7), _officialWeekendDays, _periodRange);
			Assert.Throws<ArgumentOutOfRangeException>(() => _target.IsValid(_bitArray, 20));
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

    [TestFixture]
    public class FreeWeekendValidatorUSTest
    {
    	private IDayOffLegalStateValidator _target;
        private MinMax<int> _periodRange;
        private BitArray _bitArray;
        private IOfficialWeekendDays _officialWeekendDays;

    	[SetUp]
        public void Setup()
    	{
            _officialWeekendDays = new OfficialWeekendDays();
            _periodRange = new MinMax<int>(7, 13);
            _bitArray = createBitArrayForTest();
        }

        [Test]
        public void VerifyValidationWeekends()
        {
            _target = new FreeWeekendValidator(new MinMax<int>(0, 0), _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(_bitArray, 10));
        }

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
                                    true
                                };
            BitArray result = new BitArray(values);
            return result;
        }
    }

    [TestFixture]
    public class FreeWeekendValidatorArabicTest
    {
    	private IDayOffLegalStateValidator _target;
        private MinMax<int> _periodRange;
        private BitArray _bitArray;
        private IOfficialWeekendDays _officialWeekendDays;

    	[SetUp]
        public void Setup()
    	{
            _officialWeekendDays = new OfficialWeekendDays();
            _periodRange = new MinMax<int>(7, 13);
            _bitArray = createBitArrayForTest();
        }

        [Test]
        public void VerifyValidationWeekends()
        {
            _target = new FreeWeekendValidator(
                new MinMax<int>(1, 1),
                _officialWeekendDays,
                _periodRange);
            Assert.IsFalse(_target.IsValid(_bitArray, 11));

            _target = new FreeWeekendValidator(
                new MinMax<int>(0, 0),
                _officialWeekendDays,
                _periodRange);
            Assert.IsTrue(_target.IsValid(_bitArray, 11));
        }

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
                                    false,
                                    false,
                                    false,
                                    true,
                                    false,
                                    false,

                                    true,
                                    true,
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