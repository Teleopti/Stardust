namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public interface ISchedulePostHint
	{
		void FillResult(HintResult hintResult, SchedulePostHintInput input);
	}
}