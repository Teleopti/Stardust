namespace Teleopti.Ccc.Win.Sikuli.Validators.TestValidators
{
	public class WarnValidator : ISikuliValidator
	{
		public SikuliValidationResult Validate()
		{
			return new SikuliValidationResult(SikuliValidationResult.ResultValue.Warn);
		}

		public string Description { get { return "Sikuli selftest validator. Result must be WARN."; } }
	}
}
