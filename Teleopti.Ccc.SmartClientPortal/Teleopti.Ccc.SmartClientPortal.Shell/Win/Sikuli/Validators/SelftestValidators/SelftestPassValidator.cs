using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.RootValidators;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.SelftestValidators
{
	internal class SelftestPassValidator : RootValidator
	{

		public override SikuliValidationResult Validate(object data, ITimeZoneGuard timeZoneGuard)
		{
			return new SikuliValidationResult(SikuliValidationResult.ResultValue.Pass);
		}

		public override string Description { get { return "Sikuli selftest validator. Result must be PASS."; } }
	}
}
