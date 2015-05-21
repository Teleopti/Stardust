using NUnit.Framework;
using Teleopti.Ccc.TestCommon;
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
			((FakeScheduleRange)FakeScheduleRange).SetValues(8,7);
			Assert.IsFalse(Target.Validate(FakeScheduleRange));
		}

		[Test]
		public void ShouldReturnFalseIfScheduleDaysOffIsNull()
		{
			FakeScheduleRange.CalculatedTargetScheduleDaysOff = null;
			Assert.IsFalse(Target.Validate(FakeScheduleRange));
		}

		[Test]
		public void ShouldReturnTrueIfTargetDayIsFullfilled()
		{
			((FakeScheduleRange)FakeScheduleRange).SetValues(6, 6);
			Assert.True(Target.Validate(FakeScheduleRange));
		}
	}
}
