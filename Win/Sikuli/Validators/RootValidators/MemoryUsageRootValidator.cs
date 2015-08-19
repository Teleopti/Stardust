using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	internal class MemoryUsageRootValidator : RootValidator
	{

		public override SikuliValidationResult Validate(object data)
		{
			AtomicValidators.Add(new MemoryUsageValidator(100, MemoryCounter.DefaultInstance()));
			return ValidateAtomicValidators(AtomicValidators);
		}
	}
}
