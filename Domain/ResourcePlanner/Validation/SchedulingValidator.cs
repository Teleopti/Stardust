using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class SchedulingValidator
	{
		private readonly IEnumerable<IScheduleValidator> _validators;

		public SchedulingValidator(IEnumerable<IScheduleValidator> validators)
		{
			_validators = validators;
		}

		public ValidationResult Validate(IEnumerable<IPerson> agents, DateOnlyPeriod period, bool fromWeb)
		{
			var result = new ValidationResult();
			foreach (var validator in _validators)
			{
				if(fromWeb || validator.AlsoRunInDesktop)
				{
					validator.FillResult(result, agents, period);
				}
			}
			return result;
		}
	}
}