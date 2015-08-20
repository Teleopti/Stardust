﻿using System;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	internal class ScheduleTeamSameStartTimeValidator : SchedulerRootValidator
	{
		public ScheduleTeamSameStartTimeValidator()
		{
			AtomicValidators.Add(new DurationValidator(TimeSpan.FromMinutes(2).Add(TimeSpan.FromSeconds(40))));
		}

		protected override SikuliValidationResult Validate(SchedulerTestData data)
		{
			AtomicValidators.Add(new SchedulerHoursWeeklyPatternValidator(data.SchedulerState, data.TotalSkill));

			return ValidateAtomicValidators(AtomicValidators);
		}
	}
}
