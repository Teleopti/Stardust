namespace Teleopti.Ccc.Win.Sikuli.Validators.TestValidators
{
	internal class WarnValidator : ISikuliValidator
	{
		public SikuliValidationResult Validate(ITestDuration duration)
		{
			return new SikuliValidationResult(SikuliValidationResult.ResultValue.Warn);
		}

		public string Description { get { return "Sikuli selftest validator. Result must be WARN."; } }
	}
}
