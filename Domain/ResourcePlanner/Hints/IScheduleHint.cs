namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public interface IScheduleHint
	{
		void FillResult(HintResult hintResult, HintInput input);
	}
}