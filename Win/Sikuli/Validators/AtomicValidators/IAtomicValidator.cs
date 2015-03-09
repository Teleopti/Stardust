using Teleopti.Ccc.Win.Sikuli.Helpers;

namespace Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators
{
	public interface IAtomicValidator
	{
		SikuliValidationResult Validate();
		string Description { get; }
	}
}
