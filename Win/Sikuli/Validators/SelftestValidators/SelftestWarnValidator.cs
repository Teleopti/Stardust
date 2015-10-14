using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.RootValidators;

namespace Teleopti.Ccc.Win.Sikuli.Validators.SelftestValidators
{
	internal class SelftestWarnValidator : RootValidator
	{

		public override SikuliValidationResult Validate(object data)
		{
			return new SikuliValidationResult(SikuliValidationResult.ResultValue.Warn);
		}

		public override string Description { get { return "Sikuli selftest validator. Result must be WARN."; } }
	}
}
