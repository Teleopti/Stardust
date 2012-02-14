using System.Collections;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest
{
    [TestFixture]
    public class WeeklyFreeWeekendDayValidatorTest
    {

        #region Variables

        private WeeklyFreeWeekendDayValidator _target;
        private IOfficialWeekendDays _officialWeekendDays;
        private BitArray _periodDays;

        #endregion

        [SetUp]
        public void Setup()
        {
            
            _periodDays = createBitArrayForTest();
        }

        [Test]
        public void VerifyIsValidSwedishCulture()
        {
            _officialWeekendDays = new OfficialWeekendDays(new CultureInfo("se-SE"));

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

        [Test]
        public void VerifyIsValidUSCulture()
        {
            _officialWeekendDays = new OfficialWeekendDays(new CultureInfo("en-US"));

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
