using Teleopti.Ccc.Win.Sikuli.Helpers;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	/// <summary>
	/// Top level validator.
	/// </summary>
	public interface IRootValidator
	{
		SikuliValidationResult Validate(object data);
		string Description { get; }
	}
}
