﻿using System;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	internal class ScheduleBlockSameStartTimeValidator : RootValidator
	{
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly IAggregateSkill _totalSkill;

		public ScheduleBlockSameStartTimeValidator(ISchedulerStateHolder schedulerState, IAggregateSkill totalSkill)
		{
			_schedulerState = schedulerState;
			_totalSkill = totalSkill;
		}

		public override SikuliValidationResult Validate(ITestDuration duration)
		{
			AtomicValidators.Add(new SchedulerHoursWeeklyPatternValidator(_schedulerState, _totalSkill));
			AtomicValidators.Add(new DurationValidator(TimeSpan.FromMinutes(4).Add(TimeSpan.FromSeconds(5)), duration));
			return ValidateAtomicValidators(AtomicValidators);
		}
	}
}
