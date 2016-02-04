using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	[TestFixture]
	[ResourcePlannerTest_DontUse]
	public class DayOffBusinessRuleValidationTest
	{
		public DayOffBusinessRuleValidation Target;
		public FakeScheduleRange ScheduleRange;
		[Test]
		public void ShouldReturnFalseIfTargetDayOffNotFullfilled()
		{
			ScheduleRange.UpdateCalcValues(7,TimeSpan.Zero);
			Assert.IsFalse(Target.Validate(ScheduleRange, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MaxValue)));
		}

		[Test]
		public void ShouldReturnTrueIfTargetDayIsFullfilled()
		{
			ScheduleRange.UpdateCalcValues(8, TimeSpan.Zero);
			Assert.True(Target.Validate(ScheduleRange, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MaxValue)));
		}
	}
}
