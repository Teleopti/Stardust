using System;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	internal class OptimizeBetweenDaysValidator : SchedulerRootValidator
	{

		protected override SikuliValidationResult Validate(SchedulerTestData schedulerData)
		{
			const double periodStandardDeviationLimit = 0.025d;
			AtomicValidators.Add(new PeriodStandardDeviationValidator(schedulerData.SchedulerState, schedulerData.TotalSkill, periodStandardDeviationLimit));
			AtomicValidators.Add(new DurationValidator(TimeSpan.FromMinutes(23).Add(TimeSpan.FromSeconds(30)), EndTimer()));
			return ValidateAtomicValidators(AtomicValidators);
		}
	}
}
