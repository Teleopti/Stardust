using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.RootValidators
{
	internal class MemoryUsageValidator : RootValidator
	{
		public override string Description
		{
			get { return "Memory usage must be under limit."; }
		}

		public override bool InstantValidation { get { return true; } }

		public override SikuliValidationResult Validate(object data, ITimeZoneGuard timeZoneGuard)
		{
			var memoryCounter = MemoryCounter.DefaultInstance();
			const double memoryConsumptionLimit = 100d;

			var result = new SikuliValidationResult();

			var currentConsumption = memoryCounter.CurrentMemoryConsumption();

			if (currentConsumption > memoryConsumptionLimit)
				result.Result = SikuliValidationResult.ResultValue.Warn;

			result.AppendResultLine("Memory",
				string.Format(CultureInfo.CurrentCulture, "{0:#} MB", memoryConsumptionLimit),
				string.Format(CultureInfo.CurrentCulture, "{0:#} MB", currentConsumption), result.Result);

			return result;
		}
	}
}
