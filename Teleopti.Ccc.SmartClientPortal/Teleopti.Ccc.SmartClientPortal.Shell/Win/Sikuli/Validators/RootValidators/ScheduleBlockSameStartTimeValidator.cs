using System;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.AtomicValidators;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.RootValidators
{
	internal class ScheduleBlockSameStartTimeValidator : SchedulerRootValidator
	{
		public ScheduleBlockSameStartTimeValidator()
		{
			AtomicValidators.Add(new DurationValidator(TimeSpan.FromMinutes(3).Add(TimeSpan.FromSeconds(15))));
		}

		protected override SikuliValidationResult Validate(SchedulerTestData data, ITimeZoneGuard timeZoneGuard)
		{
			AtomicValidators.Add(new SchedulerHoursWeeklyPatternValidator(data.SchedulerState, data.TotalSkill));
			return ValidateAtomicValidators(AtomicValidators, timeZoneGuard);
		}
	}
}
