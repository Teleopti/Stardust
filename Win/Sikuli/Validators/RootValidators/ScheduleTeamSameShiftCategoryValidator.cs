﻿using System;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	internal class ScheduleTeamSameShiftCategoryValidator : RootValidator
	{
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly IAggregateSkill _totalSkill;

		public ScheduleTeamSameShiftCategoryValidator(ISchedulerStateHolder schedulerState, IAggregateSkill totalSkill)
		{
			_schedulerState = schedulerState;
			_totalSkill = totalSkill;
		}

		public override SikuliValidationResult Validate(object data)
		{
			AtomicValidators.Add(new SchedulerHoursWeeklyPatternValidator(_schedulerState, _totalSkill));
			var duration = data as ITestDuration;
			AtomicValidators.Add(new DurationValidator(TimeSpan.FromMinutes(2).Add(TimeSpan.FromSeconds(25)), duration));
			return ValidateAtomicValidators(AtomicValidators);
		}
	}
}
