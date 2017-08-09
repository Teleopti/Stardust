using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class DoNothingScheduleDayChangeCallBack : IScheduleDayChangeCallback
	{
		public void ScheduleDayBeforeChanging()
		{
		}

		public void ScheduleDayChanged(IScheduleDay partBefore)
		{
		}
	}
}