using System;
using System.Collections;
using NUnit.Framework;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest
{
    [TestFixture]
    public class WeeklyFreeWeekendDayValidatorTest
    {
    	private WeeklyFreeWeekendDayValidator _target;
        private IOfficialWeekendDays _officialWeekendDays;
        private BitArray _periodDays;
    	
    	[SetUp]
        public void Setup()
    	{
			_officialWeekendDays = new OfficialWeekendDays();
            _periodDays = createBitArrayForTest();
        }

        [Test]
        public void VerifyIsValidSwedishCulture()
        {
            MinMax<int> periodRange = new MinMax<int>(8, 19);
            MinMax<int> minMaxWeekEndDay = new MinMax<int>(1, 2);
            _target = new WeeklyFreeWeekendDayValidator(minMaxWeekEndDay, _officialWeekendDays, periodRange);
            Assert.IsTrue(_target.IsValid(_periodDays, 13));

            periodRange = new MinMax<int>(8, 19);
            minMaxWeekEndDay = new MinMax<int>(2, 2);
            _target = new WeeklyFreeWeekendDayValidator(minMaxWeekEndDay, _officialWeekendDays, periodRange);
            Assert.IsFalse(_target.IsValid(_periodDays, 13));

            periodRange = new MinMax<int>(0, 19);
            minMaxWeekEndDay = new MinMax<int>(1, 2);
            _target = new WeeklyFreeWeekendDayValidator(minMaxWeekEndDay, _officialWeekendDays, periodRange);
            Assert.IsFalse(_target.IsValid(_periodDays, 13));
        }

        [Test, Ignore("Tamas will have a look at this failing test.")]
        public void VerifyIsValidUSCulture()
        {
            MinMax<int> periodRange = new MinMax<int>(8, 19);
            MinMax<int> minMaxWeekEndDay = new MinMax<int>(1, 2);
            _target = new WeeklyFreeWeekendDayValidator(minMaxWeekEndDay, _officialWeekendDays, periodRange);
            Assert.IsTrue(_target.IsValid(_periodDays, 13));

            periodRange = new MinMax<int>(8, 19);
            minMaxWeekEndDay = new MinMax<int>(2, 2);
            _target = new WeeklyFreeWeekendDayValidator(minMaxWeekEndDay, _officialWeekendDays, periodRange);
            Assert.IsFalse(_target.IsValid(_periodDays, 13));

            periodRange = new MinMax<int>(0, 19);
            minMaxWeekEndDay = new MinMax<int>(1, 2);
            _target = new WeeklyFreeWeekendDayValidator(minMaxWeekEndDay, _officialWeekendDays, periodRange);
            Assert.IsTrue(_target.IsValid(_periodDays, 13));

            periodRange = new MinMax<int>(8, 26);
            minMaxWeekEndDay = new MinMax<int>(1, 2);
            _target = new WeeklyFreeWeekendDayValidator(minMaxWeekEndDay, _officialWeekendDays, periodRange);
            Assert.IsFalse(_target.IsValid(_periodDays, 13));
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

                                    true,
                                    false,
                                    false,
                                    false,
                                    false,
                                    true,
                                    true,

                                    true,
                                    false,
                                    true,
                                    false,
                                    false,
                                    true,
                                    false,

                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    true,
                                    false
                                };
            BitArray result = new BitArray(values);
            return result;
        }
    }
}
