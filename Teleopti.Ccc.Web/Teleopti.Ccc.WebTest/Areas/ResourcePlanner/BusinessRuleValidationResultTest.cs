using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	public class DayOffBusinessRuleValidationTest
	{
		[Test]
		public void ShouldReturnFalseIfTargetDayOffNotFullfilled()
		{
			var range = new FakeScheduleRange(new FakeScheduleDictionary(), new ScheduleParameters(new Scenario("_"), new Person(), new DateTimePeriod()));
			range.UpdateCalcValues(7, TimeSpan.Zero);
			new DayOffBusinessRuleValidation().Validate(range, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MaxValue))
				.Should().Be.False();
		}

		[Test]
		public void ShouldReturnTrueIfTargetDayIsFullfilled()
		{
			var range = new FakeScheduleRange(new FakeScheduleDictionary(), new ScheduleParameters(new Scenario("_"), new Person(), new DateTimePeriod()));
			range.UpdateCalcValues(8, TimeSpan.Zero);
			new DayOffBusinessRuleValidation().Validate(range, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MaxValue))
				.Should().Be.True();
		}
	}
}
