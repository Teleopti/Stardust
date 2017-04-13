﻿using System;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.AtomicValidators;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.RootValidators
{
	internal class OptimizeBlockSameStartTimeValidator : SchedulerRootValidator
	{
		public OptimizeBlockSameStartTimeValidator()
		{
			AtomicValidators.Add(new DurationValidator(TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(50))));
		}

		protected override SikuliValidationResult Validate(SchedulerTestData data)
		{
			const double periodStandardDeviationLimit = 0.05d;
			AtomicValidators.Add(new PeriodStandardDeviationValidator(data.SchedulerState, data.TotalSkill, periodStandardDeviationLimit));
			return ValidateAtomicValidators(AtomicValidators);
		}
	}
}
