namespace Teleopti.Ccc.Win.Sikuli
{
	public interface ISikuliValidator
	{
		SikuliValidationResult Validate(ITestDuration duration);
		string Description { get; }
	}
}
