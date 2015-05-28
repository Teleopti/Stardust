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
			((FakeScheduleRange)FakeScheduleRange).UpdateCalcValues(7,TimeSpan.Zero);
			//FakeScheduleRange will return 8 target days off and 8 hours target time
			Assert.IsFalse(Target.Validate(FakeScheduleRange, new DateOnlyPeriod()));
		}

		[Test]
		public void ShouldReturnTrueIfTargetDayIsFullfilled()
		{
			((FakeScheduleRange)FakeScheduleRange).UpdateCalcValues(8, TimeSpan.Zero);
			//FakeScheduleRange will return 8 target days off and 8 hours target time
			Assert.True(Target.Validate(FakeScheduleRange, new DateOnlyPeriod()));
		}
	}
}
