using System.Collections.Generic;
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
            IList<int> weekEndIndexes = _target.WeekendDayIndexesRelativeStartDayOfWeek();
            Assert.AreEqual(5, weekEndIndexes[0]);
            Assert.AreEqual(6, weekEndIndexes[1]);
        }

        [Test]
        public void VerifyWithUSRules()
        {
            IList<int> weekendIndexes = _target.WeekendDayIndexesRelativeStartDayOfWeek();
			Assert.AreEqual(5, weekendIndexes[0]);
			Assert.AreEqual(6, weekendIndexes[1]);
        }

        [Test]
        public void VerifyWithArabicRules()
        {
            IList<int> weekendIndexes = _target.WeekendDayIndexesRelativeStartDayOfWeek();
			Assert.AreEqual(5, weekendIndexes[0]);
			Assert.AreEqual(6, weekendIndexes[1]);
        }
    }
}
