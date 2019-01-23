using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class OfficialWeekendDaysTest
    {
        private OfficialWeekendDays _target;

    	[SetUp]
		public void Setup()
		{
			_target = new OfficialWeekendDays();
		}

        [Test]
        public void VerifyWithDefaultRules()
        {
            var weekEndIndexes = _target.WeekendDayIndexesRelativeStartDayOfWeek();
			Assert.AreEqual(5, weekEndIndexes.ElementAt(0));
			Assert.AreEqual(6, weekEndIndexes.ElementAt(1));
		}

        [Test]
        public void VerifyWithUSRules()
        {
            var weekendIndexes = _target.WeekendDayIndexesRelativeStartDayOfWeek();
			Assert.AreEqual(5, weekendIndexes.ElementAt(0));
			Assert.AreEqual(6, weekendIndexes.ElementAt(1));
		}

        [Test]
        public void VerifyWithArabicRules()
        {
            var weekendIndexes = _target.WeekendDayIndexesRelativeStartDayOfWeek();
			Assert.AreEqual(5, weekendIndexes.ElementAt(0));
			Assert.AreEqual(6, weekendIndexes.ElementAt(1));
        }
    }
}
