using NUnit.Framework;
using SharpTestsEx;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [TestFixture]
    public class UnderstaffingDetailsTest
    {
        private UnderstaffingDetails target;

        [SetUp]
        public void Setup()
        {
            target = new UnderstaffingDetails();
            target.AddUnderstaffingDay(new DateOnly(2012,1,8));
            target.AddUnderstaffingDay(new DateOnly(2012,1,9));
            target.AddSeriousUnderstaffingDay(new DateOnly(2012,1,9));
            target.AddUnderstaffingTime(new TimePeriod(1,0,2,0));
            target.AddUnderstaffingTime(new TimePeriod(2,0,3,0));
            target.AddSeriousUnderstaffingTime(new TimePeriod(2,0,3,0));
        }

        [Test]
        public void ShouldHaveUnderstaffingDaysSetTwice()
        {
            target.UnderstaffingDays.Count().Should().Be.EqualTo(2);
        }

        [Test]
        public void ShouldHaveSeriousUnderstaffingDaySetOnce()
        {
            target.SeriousUnderstaffingDays.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotHaveDuplicateUnderstaffingDay()
		{
			var testDate = new DateOnly(2012, 12, 31);
			target.AddUnderstaffingDay(testDate);
			target.AddUnderstaffingDay(testDate);
			target.UnderstaffingDays.Count(x => x == testDate).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotHaveDuplicateSeriousUnderstaffingDay()
		{
			var testDate = new DateOnly(2012, 12, 31);
			target.AddSeriousUnderstaffingDay(testDate);
			target.AddSeriousUnderstaffingDay(testDate);
			target.SeriousUnderstaffingDays.Count(x => x == testDate).Should().Be.EqualTo(1);
		}

        [Test]
        public void ShouldHaveUnderstaffingTimesSetTwice()
        {
            target.UnderstaffingTimes.Count().Should().Be.EqualTo(2);
        }

        [Test]
        public void ShouldHaveSeriousUnderstaffingTimeSetOnce()
        {
            target.SeriousUnderstaffingTimes.Count().Should().Be.EqualTo(1);
        }
    }
}
