using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	internal class MemoryUsageValidator : IRootValidator
	{
		public string Description
		{
			get { return "Memory usage must be under limit."; }
		}

		public bool ExplicitValidation { get { return false; } }


		public SikuliValidationResult Validate(object data)
		{
			var memoryCounter = MemoryCounter.DefaultInstance();
			var memoryUsageLimit = 70d;

			var result = new SikuliValidationResult();

			if (memoryCounter.CurrentMemoryConsumption() > memoryUsageLimit)
				result.Result = SikuliValidationResult.ResultValue.Warn;

			result.AppendResultLine("Memory",
				string.Format(CultureInfo.CurrentCulture, "{0:#} MB", memoryUsageLimit),
				string.Format(CultureInfo.CurrentCulture, "{0:#} MB", memoryCounter.CurrentMemoryConsumption()), result.Result);

			return result;
		}
	}
}
