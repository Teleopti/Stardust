﻿using System.Globalization;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.Win.Sikuli.Helpers;

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
