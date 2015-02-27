namespace Teleopti.Ccc.Win.Sikuli.Validators.TestValidators
{
	internal class PassValidator : ISikuliValidator
	{
		public SikuliValidationResult Validate(ITestDuration duration)
		{
			return new SikuliValidationResult(SikuliValidationResult.ResultValue.Pass);
		}

		public string Description { get { return "Sikuli selftest validator. Result must be PASS."; } }
	}
}
