namespace Teleopti.Ccc.Win.Sikuli.Validators.TestValidators
{
	public class FailValidator : ISikuliValidator
	{
		public SikuliValidationResult Validate()
		{
			return new SikuliValidationResult(SikuliValidationResult.ResultValue.Fail);
		}

		public string Description { get { return "Sikuli selftest validator. Result must be FAIL."; } }
	}
}
