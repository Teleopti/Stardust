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

		public ValidationResult Validate(IScheduleDictionary schedules, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			var result = new ValidationResult();
			foreach (var validator in _validators)
			{
				validator.FillResult(result, schedules, agents, period);
			}
			return result;
		}
	}
}