using Teleopti.Ccc.Win.Sikuli.Helpers;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	/// <summary>
	/// Top level Sikuli validator interface.
	/// </summary>
	public interface IRootValidator
	{
		SikuliValidationResult Validate(ITestDuration duration);
		string Description { get; }
	}
}
