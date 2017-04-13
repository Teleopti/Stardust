using System;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators;

namespace Teleopti.Ccc.Win.Sikuli.Validators.SelftestValidators
{
	internal class SelftestAtomicValidator : IAtomicValidator
	{
		private readonly SikuliValidationResult.ResultValue _result;

		public SelftestAtomicValidator(SikuliValidationResult.ResultValue result)
		{
			_result = result;
		}

		public SikuliValidationResult Validate()
		{
			return new SikuliValidationResult(_result);
		}

		public string Description { get { return String.Format("Atomic test validator. Result must be {0}.", _result); } }
	}
}
