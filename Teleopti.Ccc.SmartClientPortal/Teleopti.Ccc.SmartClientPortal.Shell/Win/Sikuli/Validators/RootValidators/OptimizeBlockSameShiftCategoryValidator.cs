using System;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.AtomicValidators;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.RootValidators
{
	internal class OptimizeBlockSameShiftCategoryValidator : SchedulerRootValidator
	{
		public OptimizeBlockSameShiftCategoryValidator()
		{
			AtomicValidators.Add(new DurationValidator(TimeSpan.FromMinutes(2).Add(TimeSpan.FromSeconds(05))));
		}

		protected override SikuliValidationResult Validate(SchedulerTestData data, ITimeZoneGuard timeZoneGuard)
		{
			const double periodStandardDeviationLimit = 0.05d;
			AtomicValidators.Add(new PeriodStandardDeviationValidator(data.SchedulerState, data.TotalSkill, periodStandardDeviationLimit));
			return ValidateAtomicValidators(AtomicValidators, timeZoneGuard);
		}
	}
}
