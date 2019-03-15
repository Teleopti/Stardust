using System;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.AtomicValidators;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.RootValidators
{
	internal class OptimizeBlockTeamSameShiftCategoryDaysOffValidator : SchedulerRootValidator
	{
		public OptimizeBlockTeamSameShiftCategoryDaysOffValidator()
		{
			AtomicValidators.Add(new DurationValidator(TimeSpan.FromMinutes(0).Add(TimeSpan.FromSeconds(45))));
		}

		protected override SikuliValidationResult Validate(SchedulerTestData data, ITimeZoneGuard timeZoneGuard)
		{
			const double periodStandardDeviationLimit = 0.108d;
			AtomicValidators.Add(new PeriodStandardDeviationValidator(data.SchedulerState, data.TotalSkill, periodStandardDeviationLimit));
			return ValidateAtomicValidators(AtomicValidators, timeZoneGuard);
		}
	}
}
