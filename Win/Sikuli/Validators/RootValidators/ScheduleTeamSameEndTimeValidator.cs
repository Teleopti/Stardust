using System;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	internal class ScheduleTeamSameEndTimeValidator : SchedulerRootValidator
	{
		public ScheduleTeamSameEndTimeValidator()
		{
			AtomicValidators.Add(new DurationValidator(TimeSpan.FromMinutes(2).Add(TimeSpan.FromSeconds(22))));
		}

		protected override SikuliValidationResult Validate(SchedulerTestData data)
		{
			AtomicValidators.Add(new SchedulerHoursWeeklyPatternValidator(data.SchedulerState, data.TotalSkill));
			return ValidateAtomicValidators(AtomicValidators);
		}
	}
}
