using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.RootValidators;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.SelftestValidators
{
	internal class SelftestRootValidator: RootValidator
	{
		public override string Description
		{
			get
			{
				return "Testing atomic validators. The result must be FAIL";
			}
		}

		public override SikuliValidationResult Validate(object data, ITimeZoneGuard timeZoneGuard)
		{
			AtomicValidators.Add(new SelftestAtomicValidator(SikuliValidationResult.ResultValue.Fail));
			AtomicValidators.Add(new SelftestAtomicValidator(SikuliValidationResult.ResultValue.Pass));
			AtomicValidators.Add(new SelftestAtomicValidator(SikuliValidationResult.ResultValue.Warn));
			return ValidateAtomicValidators(AtomicValidators, timeZoneGuard);
		}
	}
}
