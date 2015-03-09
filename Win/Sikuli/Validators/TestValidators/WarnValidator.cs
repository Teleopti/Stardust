﻿using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.RootValidators;

namespace Teleopti.Ccc.Win.Sikuli.Validators.TestValidators
{
	internal class WarnValidator : IRootValidator
	{
		public SikuliValidationResult Validate(ITestDuration duration)
		{
			return new SikuliValidationResult(SikuliValidationResult.ResultValue.Warn);
		}

		public string Description { get { return "Sikuli selftest validator. Result must be WARN."; } }
	}
}
