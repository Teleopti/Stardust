using System;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	internal class ScheduleBlockSameStartTimeValidator : SchedulerRootValidator
	{
		public ScheduleBlockSameStartTimeValidator()
		{
			AtomicValidators.Add(new DurationValidator(TimeSpan.FromMinutes(5).Add(TimeSpan.FromSeconds(15))));
		}

		protected override SikuliValidationResult Validate(SchedulerTestData data)
		{
			AtomicValidators.Add(new SchedulerHoursWeeklyPatternValidator(data.SchedulerState, data.TotalSkill));
			return ValidateAtomicValidators(AtomicValidators);
		}
	}
}
