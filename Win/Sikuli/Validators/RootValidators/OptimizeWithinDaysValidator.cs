﻿using System;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	internal class OptimizeWithinDaysValidator : SchedulerRootValidator
	{
		public OptimizeWithinDaysValidator()
		{
			AtomicValidators.Add(new DurationValidator(TimeSpan.FromMinutes(6).Add(TimeSpan.FromSeconds(30))));
		}

		protected override SikuliValidationResult Validate(SchedulerTestData data)
		{
			const double dailyStandardDeviationSumlimit = 4.65d;
			AtomicValidators.Add(new DailyStandardDeviationValidator(data.SchedulerState, data.TotalSkill, dailyStandardDeviationSumlimit));
			AtomicValidators.Add(new DayOffAndContractTimeValidator(data.SchedulerState));
			return ValidateAtomicValidators(AtomicValidators);
		}
	}
}
