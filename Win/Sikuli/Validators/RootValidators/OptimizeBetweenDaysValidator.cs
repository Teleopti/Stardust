using System;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	internal class OptimizeBetweenDaysValidator : SchedulerRootValidator
	{
		public OptimizeBetweenDaysValidator()
		{
			AtomicValidators.Add(new DurationValidator(TimeSpan.FromMinutes(10).Add(TimeSpan.FromSeconds(10))));
		}

		protected override SikuliValidationResult Validate(SchedulerTestData data)
		{
			const double periodStandardDeviationLimit = 0.025d;
			AtomicValidators.Add(new PeriodStandardDeviationValidator(data.SchedulerState, data.TotalSkill, periodStandardDeviationLimit));
			return ValidateAtomicValidators(AtomicValidators);
		}
	}
}
