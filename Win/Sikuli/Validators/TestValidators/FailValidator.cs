namespace Teleopti.Ccc.Win.Sikuli.Validators.TestValidators
{
	internal class FailValidator : ISikuliValidator
	{
		public SikuliValidationResult Validate(ITestDuration duration)
		{
			return new SikuliValidationResult(SikuliValidationResult.ResultValue.Fail);
		}

		public string Description { get { return "Sikuli selftest validator. Result must be FAIL."; } }
	}
}
