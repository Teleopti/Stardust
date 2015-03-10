using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.RootValidators;

namespace Teleopti.Ccc.Win.Sikuli.Validators.SelftestValidators
{
	internal class PassValidator : IRootValidator
	{
		public SikuliValidationResult Validate(ITestDuration duration)
		{
			return new SikuliValidationResult(SikuliValidationResult.ResultValue.Pass);
		}

		public string Description { get { return "Sikuli selftest validator. Result must be PASS."; } }
	}
}
