using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.RootValidators;

namespace Teleopti.Ccc.Win.Sikuli.Validators.SelftestValidators
{
	internal class FailValidator : IRootValidator
	{
		public SikuliValidationResult Validate(ITestDuration duration)
		{
			return new SikuliValidationResult(SikuliValidationResult.ResultValue.Fail);
		}

		public string Description { get { return "Sikuli selftest validator. Result must be FAIL."; } }
	}
}
