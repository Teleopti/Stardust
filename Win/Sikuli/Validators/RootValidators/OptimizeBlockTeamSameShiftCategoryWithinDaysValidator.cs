using System;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	internal class OptimizeBlockTeamSameShiftCategoryWithinDaysValidator : SchedulerRootValidator
	{
		public OptimizeBlockTeamSameShiftCategoryWithinDaysValidator()
		{
			AtomicValidators.Add(new DurationValidator(TimeSpan.FromMinutes(3).Add(TimeSpan.FromSeconds(55))));
		}

		protected override SikuliValidationResult Validate(SchedulerTestData data)
		{
			const double periodStandardDeviationLimit = 0.08d;
			AtomicValidators.Add(new PeriodStandardDeviationValidator(data.SchedulerState, data.TotalSkill, periodStandardDeviationLimit));
			return ValidateAtomicValidators(AtomicValidators);
		}
	}
}
