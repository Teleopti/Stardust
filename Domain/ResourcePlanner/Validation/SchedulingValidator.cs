using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class SchedulingValidator
	{
		private readonly IEnumerable<IScheduleValidator> _validators;

		public SchedulingValidator(IEnumerable<IScheduleValidator> validators)
		{
			_validators = validators;
		}

		public ValidationResult Validate(ValidationInput validationInput)
		{
			var result = new ValidationResult();
			foreach (var validator in _validators)
			{
				validator.FillResult(result, validationInput);
			}
			return result;
		}
	}
}