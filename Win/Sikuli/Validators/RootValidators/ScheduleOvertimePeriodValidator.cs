using System;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	internal class ScheduleOvertimePeriodValidator : SchedulerRootValidator
	{
		public ScheduleOvertimePeriodValidator()
		{
			AtomicValidators.Add(new DurationValidator(TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(5))));
		}

		protected override SikuliValidationResult Validate(SchedulerTestData data)
		{
			const double periodStandardDeviationLimit = 0.03d;
			AtomicValidators.Add(new PeriodStandardDeviationValidator(data.SchedulerState, data.TotalSkill, periodStandardDeviationLimit));
			return ValidateAtomicValidators(AtomicValidators);
		}
	}
}
