using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.AtomicValidators
{
	/// <summary>
	/// One step sub-validator.
	/// </summary>
	/// <remarks>
	/// Used only from root validator.
	/// </remarks>
	public interface IAtomicValidator
	{
		SikuliValidationResult Validate();
		string Description { get; }
	}
}
