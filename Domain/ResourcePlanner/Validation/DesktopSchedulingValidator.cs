using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class DesktopSchedulingValidator
	{
		private readonly IBasicSchedulingValidator _basicSchedulingValidator;

		public DesktopSchedulingValidator(IBasicSchedulingValidator basicSchedulingValidator)
		{
			_basicSchedulingValidator = basicSchedulingValidator;
		}

		public ValidationResult Validate(IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			return _basicSchedulingValidator.Validate(new ValidationParameters {People = agents.ToArray(), Period = period});
		}
	}
}