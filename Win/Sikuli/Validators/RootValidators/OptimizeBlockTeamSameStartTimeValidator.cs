using System;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	internal class OptimizeBlockTeamSameStartTimeValidator : SchedulerRootValidator
	{
		public OptimizeBlockTeamSameStartTimeValidator()
		{
			AtomicValidators.Add(new DurationValidator(TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(32))));
		}

		protected override SikuliValidationResult Validate(SchedulerTestData data)
		{
			const double periodStandardDeviationLimit = 0.09d;
			AtomicValidators.Add(new PeriodStandardDeviationValidator(data.SchedulerState, data.TotalSkill, periodStandardDeviationLimit));
			return ValidateAtomicValidators(AtomicValidators);
		}
	}
}
