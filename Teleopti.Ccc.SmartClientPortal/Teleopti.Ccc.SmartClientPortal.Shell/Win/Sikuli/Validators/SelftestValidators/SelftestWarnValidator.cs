using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.RootValidators;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.SelftestValidators
{
	internal class SelftestWarnValidator : RootValidator
	{

		public override SikuliValidationResult Validate(object data, ITimeZoneGuard timeZoneGuard)
		{
			return new SikuliValidationResult(SikuliValidationResult.ResultValue.Warn);
		}

		public override string Description { get { return "Sikuli selftest validator. Result must be WARN."; } }
	}
}
