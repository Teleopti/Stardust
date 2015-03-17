using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.RootValidators;

namespace Teleopti.Ccc.Win.Sikuli.Validators.SelftestValidators
{
	internal class TestRootValidator: RootValidator
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
			AtomicValidators.Add(new TestAtomicValidator(SikuliValidationResult.ResultValue.Fail));
			AtomicValidators.Add(new TestAtomicValidator(SikuliValidationResult.ResultValue.Pass));
			AtomicValidators.Add(new TestAtomicValidator(SikuliValidationResult.ResultValue.Warn));
			return ValidateAtomicValidators(AtomicValidators);
		}
	}
}
