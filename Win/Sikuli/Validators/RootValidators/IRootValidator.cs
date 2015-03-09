using Teleopti.Ccc.Win.Sikuli.Helpers;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	public interface IRootValidator
	{
		SikuliValidationResult Validate(ITestDuration duration);
		string Description { get; }
	}
}
