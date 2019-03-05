using System;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.AtomicValidators;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.RootValidators
{
	internal class ScheduleBlockTeamSameStartTimeValidator : SchedulerRootValidator
	{
		public ScheduleBlockTeamSameStartTimeValidator()
		{
			AtomicValidators.Add(new DurationValidator(TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(59))));
		}

		protected override SikuliValidationResult Validate(SchedulerTestData data, ITimeZoneGuard timeZoneGuard)
		{
			AtomicValidators.Add(new SchedulerHoursWeeklyPatternValidator(data.SchedulerState, data.TotalSkill));
			return ValidateAtomicValidators(AtomicValidators, timeZoneGuard);
		}
	}
}
