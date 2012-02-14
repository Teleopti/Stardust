using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class OfficialWeekendDaysTest
    {
        private OfficialWeekendDays _target;

        [Test]
        public void VerifyWithDefaultRules()
        {
            CultureInfo culture = new CultureInfo("se-SE");
            _target = new OfficialWeekendDays(culture);
            Assert.AreEqual(DayOfWeek.Monday, _target.WeekStartDay);

            IList<DayOfWeek> weekEndDays = _target.WeekendDays();
            Assert.AreEqual(weekEndDays[0], DayOfWeek.Saturday);
            Assert.AreEqual(weekEndDays[1], DayOfWeek.Sunday);

            IList<int> weekEndIndexes = _target.WeekendDayIndexes();
            Assert.AreEqual(weekEndIndexes[0], 5);
            Assert.AreEqual(weekEndIndexes[1], 6);
        }

        [Test]
        public void VerifyWithUSRules()
        {
            CultureInfo culture = new CultureInfo("en-US");
            _target = new OfficialWeekendDays(culture);
            Assert.AreEqual(DayOfWeek.Sunday, _target.WeekStartDay);

            IList<DayOfWeek> weekendDays = _target.WeekendDays();
            Assert.AreEqual(weekendDays[0], DayOfWeek.Sunday);
            Assert.AreEqual(weekendDays[1], DayOfWeek.Saturday);

            IList<int> weekendIndexes = _target.WeekendDayIndexes();
            Assert.AreEqual(weekendIndexes[0], 0);
            Assert.AreEqual(weekendIndexes[1], 6);
        }

        [Test]
        public void VerifyWithArabicRules()
        {
            CultureInfo culture = new CultureInfo("ar-AE");
            _target = new OfficialWeekendDays(culture);
            Assert.AreEqual(DayOfWeek.Saturday, _target.WeekStartDay);

            IList<DayOfWeek> weekendDays = _target.WeekendDays();
            Assert.AreEqual(weekendDays[0], DayOfWeek.Saturday);
            Assert.AreEqual(weekendDays[1], DayOfWeek.Friday);

            IList<int> weekendIndexes = _target.WeekendDayIndexes();
            Assert.AreEqual(weekendIndexes[0], 0);
            Assert.AreEqual(weekendIndexes[1], 6);
        }
    }
}
