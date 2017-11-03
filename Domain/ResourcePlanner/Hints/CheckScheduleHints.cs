using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class CheckScheduleHints
	{
		private readonly IEnumerable<IScheduleHint> _validators;

		public CheckScheduleHints(IEnumerable<IScheduleHint> validators)
		{
			_validators = validators;
		}

		public HintResult Execute(HintInput hintInput)
		{
			var result = new HintResult();
			foreach (var validator in _validators)
			{
				validator.FillResult(result, hintInput);
			}
			return result;
		}
	}
}