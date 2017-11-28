using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	public class DayOffBusinessRuleValidationTest
	{
		[Test]
		public void ShouldReturnFalseIfTargetDayOffNotFullfilled()
		{
			var targetSummary = new TargetScheduleSummary
			{
				TargetDaysOff = 8
			};
			var currentSummary = new CurrentScheduleSummary
			{
				NumberOfDaysOff = 7
			};
			new DayOffBusinessRuleValidation().Validate(targetSummary, currentSummary).Should().Be.False();
		}

		[Test]
		public void ShouldReturnTrueIfTargetDayIsFullfilled()
		{
			var targetSummary = new TargetScheduleSummary
			{
				TargetDaysOff = 8
			};
			var currentSummary = new CurrentScheduleSummary
			{
				NumberOfDaysOff = 8
			};

			new DayOffBusinessRuleValidation().Validate(targetSummary, currentSummary).Should().Be.True();
		}
	}
}
