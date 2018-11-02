namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public interface ISchedulePreHint
	{
		void FillResult(HintResult hintResult, ScheduleHintInput input);
	}
}