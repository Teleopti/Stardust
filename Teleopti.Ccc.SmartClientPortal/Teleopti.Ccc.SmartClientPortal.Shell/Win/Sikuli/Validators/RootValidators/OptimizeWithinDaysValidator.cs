using System;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.AtomicValidators;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.RootValidators
{
	internal class OptimizeWithinDaysValidator : SchedulerRootValidator
	{
		public OptimizeWithinDaysValidator()
		{
			AtomicValidators.Add(new DurationValidator(TimeSpan.FromMinutes(6).Add(TimeSpan.FromSeconds(30))));
		}

		protected override SikuliValidationResult Validate(SchedulerTestData data, ITimeZoneGuard timeZoneGuard)
		{
			const double dailyStandardDeviationSumlimit = 5.2d;

			AtomicValidators.Add(new DailyStandardDeviationValidator(data.SchedulerState, data.TotalSkill, dailyStandardDeviationSumlimit));
			AtomicValidators.Add(new DayOffAndContractTimeValidator(data.SchedulerState));
			return ValidateAtomicValidators(AtomicValidators, timeZoneGuard);
		}
	}
}
