using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class DoNothingScheduleDayChangeCallBack : IScheduleDayChangeCallback
	{
		public void ScheduleDayBeforeChanging()
		{
		}

		public void ScheduleDayChanged(IScheduleDay partBefore, IScheduleDay partAfter)
		{
		}
	}
}