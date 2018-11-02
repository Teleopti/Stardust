using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class CheckScheduleHints
	{
		private readonly IEnumerable<ISchedulePostHint> _postValidators;
		private readonly IEnumerable<ISchedulePreHint> _preValidators;

		public CheckScheduleHints(IEnumerable<ISchedulePostHint> postValidators, IEnumerable<ISchedulePreHint> preValidators)
		{
			_postValidators = postValidators;
			_preValidators = preValidators;
		}

		public HintResult Execute(ScheduleHintInput scheduleHintInput)
		{
			var result = new HintResult();
			
			foreach (var validator in _preValidators)
			{
				validator.FillResult(result, scheduleHintInput);
			}
			
			if (scheduleHintInput is SchedulePostHintInput input)
			{
				foreach (var validator in _postValidators)
				{
					validator.FillResult(result, input);
				}
			}
			return result;
		}
	}
}