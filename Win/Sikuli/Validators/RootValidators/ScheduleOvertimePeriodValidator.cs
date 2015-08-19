using System;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	internal class ScheduleOvertimePeriodValidator : RootValidator
	{
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly IAggregateSkill _totalSkill;

		public ScheduleOvertimePeriodValidator(ISchedulerStateHolder schedulerState, IAggregateSkill totalSkill)
		{
			_schedulerState = schedulerState;
			_totalSkill = totalSkill;
		}

		public override SikuliValidationResult Validate(object data)
		{
			const double periodStandardDeviationLimit = 0.03d;
			AtomicValidators.Add(new PeriodStandardDeviationValidator(_schedulerState, _totalSkill, periodStandardDeviationLimit));
			var duration = data as ITestDuration;
			AtomicValidators.Add(new DurationValidator(TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(5)), duration));
			return ValidateAtomicValidators(AtomicValidators);
		}
	}
}
