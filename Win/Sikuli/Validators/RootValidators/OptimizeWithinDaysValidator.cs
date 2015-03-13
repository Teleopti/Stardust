using System;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	internal class OptimizeWithinDaysValidator : RootValidator
	{
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly IAggregateSkill _totalSkill;

		public OptimizeWithinDaysValidator(ISchedulerStateHolder schedulerState, IAggregateSkill totalSkill)
		{
			_schedulerState = schedulerState;
			_totalSkill = totalSkill;
		}

		public override SikuliValidationResult Validate(ITestDuration duration)
		{
			const double dailyStandardDeviationSumlimit = 4.65d;
			AtomicValidators.Add(new DailyStandardDeviationValidator(_schedulerState, _totalSkill, dailyStandardDeviationSumlimit));
			AtomicValidators.Add(new DayOffAndContractTimeValidator(_schedulerState));
			AtomicValidators.Add(new DurationValidator(TimeSpan.FromMinutes(6).Add(TimeSpan.FromSeconds(50)), duration));
			return ValidateAtomicValidators(AtomicValidators);
		}
	}
}
