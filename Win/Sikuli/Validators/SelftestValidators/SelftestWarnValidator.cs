﻿using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.RootValidators;

namespace Teleopti.Ccc.Win.Sikuli.Validators.SelftestValidators
{
	internal class SelftestWarnValidator : IRootValidator
	{
		public SikuliValidationResult Validate(object data)
		{
			return new SikuliValidationResult(SikuliValidationResult.ResultValue.Warn);
		}

		public string Description { get { return "Sikuli selftest validator. Result must be WARN."; } }
	}
}
