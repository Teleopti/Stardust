using NUnit.Framework;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	[TestFixture]
	[ResourcePlannerTest]
	public class DayOffBusinessRuleValidationTest
	{
		public DayOffBusinessRuleValidation Target;
		public IScheduleRange FakeScheduleRange;
		[Test]
		public void ShouldReturnFalseIfTargetDayOffNotFullfilled()
		{
			FakeScheduleRange.CalculatedScheduleDaysOff = 7;
			FakeScheduleRange.CalculatedTargetScheduleDaysOff = 8;
			Assert.IsFalse(Target.Validate(FakeScheduleRange));
		}

		[Test]
		public void ShouldReturnFalseIfScheduleDaysOffIsNull()
		{
			FakeScheduleRange.CalculatedScheduleDaysOff = 8;
			FakeScheduleRange.CalculatedTargetScheduleDaysOff = null;
			Assert.IsFalse(Target.Validate(FakeScheduleRange));
		}

		[Test]
		public void ShouldReturnTrueIfTargetDayIsFullfilled()
		{
			FakeScheduleRange.CalculatedScheduleDaysOff = 6;
			FakeScheduleRange.CalculatedTargetScheduleDaysOff = 6;
			Assert.True(Target.Validate(FakeScheduleRange));
		}
	}
}
