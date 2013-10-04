using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class EmptyScheduleDayChangeCallback : IScheduleDayChangeCallback
	{
		public void ScheduleDayChanging(IScheduleDay partBefore)
		{
		}

		public void ScheduleDayChanged(IScheduleDay partAfter)
		{
		}
	}
}