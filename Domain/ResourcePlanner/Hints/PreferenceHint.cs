using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class PreferenceHint : IScheduleHint
	{
		public void FillResult(HintResult hintResult, HintInput input)
		{
			hintResult.Add(new PersonHintError(), null, ValidationResourceType.Preferences);
		}
	}
}