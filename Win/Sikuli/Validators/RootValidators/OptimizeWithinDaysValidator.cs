using System;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	internal class OptimizeWithinDaysValidator : SchedulerRootValidator
	{
		protected override SikuliValidationResult Validate(SchedulerTestData schedulerData)
		{
			const double dailyStandardDeviationSumlimit = 4.65d;
			AtomicValidators.Add(new DailyStandardDeviationValidator(schedulerData.SchedulerState, schedulerData.TotalSkill, dailyStandardDeviationSumlimit));
			AtomicValidators.Add(new DayOffAndContractTimeValidator(schedulerData.SchedulerState));
			AtomicValidators.Add(new DurationValidator(TimeSpan.FromMinutes(6).Add(TimeSpan.FromSeconds(30)), EndTimer()));
			return ValidateAtomicValidators(AtomicValidators);
		}
	}
}
