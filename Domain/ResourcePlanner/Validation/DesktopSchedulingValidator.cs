using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class DesktopSchedulingValidator
	{
		private readonly SchedulingValidator _schedulingValidator;

		public DesktopSchedulingValidator(SchedulingValidator schedulingValidator)
		{
			_schedulingValidator = schedulingValidator;
		}

		public ValidationResult Validate(IEnumerable<IPerson> agents, DateOnlyPeriod period, IEnumerable<SkillMissingForecast> existingForecast)
		{
			return _schedulingValidator.Validate(agents, period, existingForecast, false);
		}
	}
}