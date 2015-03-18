﻿using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.RootValidators;

namespace Teleopti.Ccc.Win.Sikuli.Validators.SelftestValidators
{
	internal class SelftestRootValidator: RootValidator
	{
		public override string Description
		{
			get
			{
				return "Testing atomic validators. The result must be FAIL";
			}
		}

		public override SikuliValidationResult Validate(ITestDuration duration)
		{
			AtomicValidators.Add(new SelftestAtomicValidator(SikuliValidationResult.ResultValue.Fail));
			AtomicValidators.Add(new SelftestAtomicValidator(SikuliValidationResult.ResultValue.Pass));
			AtomicValidators.Add(new SelftestAtomicValidator(SikuliValidationResult.ResultValue.Warn));
			return ValidateAtomicValidators(AtomicValidators);
		}
	}
}
