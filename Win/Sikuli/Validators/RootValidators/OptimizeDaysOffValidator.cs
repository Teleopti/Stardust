using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	internal class OptimizeDaysOffValidator : SchedulerRootValidator
	{
		public OptimizeDaysOffValidator()
		{
			AtomicValidators.Add(new DurationValidator(TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(40))));
		}

		protected override SikuliValidationResult Validate(SchedulerTestData data)
		{
			const double periodStandardDeviationLimit = 0.05d;
			AtomicValidators.Add(new PeriodStandardDeviationValidator(data.SchedulerState, data.TotalSkill, periodStandardDeviationLimit));
			return ValidateAtomicValidators(AtomicValidators);
		}
	}
}
