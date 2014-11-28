using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators.TestValidators
{
	public class PassValidator : ISikuliValidator
	{
		public SikuliValidationResult Validate()
		{
			return new SikuliValidationResult(SikuliValidationResult.ResultValue.Pass);
		}
	}
}
