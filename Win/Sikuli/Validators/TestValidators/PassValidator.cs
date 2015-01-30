namespace Teleopti.Ccc.Win.Sikuli.Validators.TestValidators
{
	public class PassValidator : ISikuliValidator
	{
		public SikuliValidationResult Validate()
		{
			return new SikuliValidationResult(SikuliValidationResult.ResultValue.Pass);
		}

		public string Description { get { return "Sikuli selftest validator. Result must be PASS."; } }
	}
}
