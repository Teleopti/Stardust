using System;
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
			((FakeScheduleRange)FakeScheduleRange).UpdateCalcValues(8, 7,TimeSpan.Zero,TimeSpan.Zero);
			//FakeScheduleRange will return 8 target days off
			Assert.IsFalse(Target.Validate(FakeScheduleRange, new DateOnlyPeriod()));
		}

		[Test]
		public void ShouldReturnTrueIfTargetDayIsFullfilled()
		{
			((FakeScheduleRange)FakeScheduleRange).UpdateCalcValues(6, 6,TimeSpan.Zero,TimeSpan.Zero);
			//FakeScheduleRange will return 8 target days off
			Assert.True(Target.Validate(FakeScheduleRange, new DateOnlyPeriod()));
		}
	}
}
