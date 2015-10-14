using Teleopti.Ccc.Win.Sikuli.Helpers;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	/// <summary>
	/// Top level validator.
	/// </summary>
	public interface IRootValidator
	{
		/// <summary>
		/// Id of the validator
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Flag to signal sikuli helper the validator waits for explicit call for validation
		/// </summary>
		bool ExplicitValidation { get; } 
		/// <summary>
		/// abstract method for running validation logic
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		SikuliValidationResult Validate(object data);
		/// <summary>
		/// User dfriendly description of validator task
		/// </summary>
		string Description { get; }
	}
}
