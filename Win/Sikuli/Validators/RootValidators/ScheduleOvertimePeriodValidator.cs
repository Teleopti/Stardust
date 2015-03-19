﻿using System;
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

		public override SikuliValidationResult Validate(ITestDuration duration)
		{
			const double periodStandardDeviationLimit = 0.03d;
			AtomicValidators.Add(new PeriodStandardDeviationValidator(_schedulerState, _totalSkill, periodStandardDeviationLimit));
			AtomicValidators.Add(new DurationValidator(TimeSpan.FromSeconds(55), duration));
			return ValidateAtomicValidators(AtomicValidators);
		}
	}
}
