using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.RootValidators
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
		/// Flag to signal sikuli helper the validator validated instantly after created
		/// </summary>
		bool InstantValidation { get; } 
		/// <summary>
		/// abstract method for running validation logic
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		SikuliValidationResult Validate(object data, ITimeZoneGuard timeZoneGuard);
		/// <summary>
		/// User dfriendly description of validator task
		/// </summary>
		string Description { get; }
	}
}
