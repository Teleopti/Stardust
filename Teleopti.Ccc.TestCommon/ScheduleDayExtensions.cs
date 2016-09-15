using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public static class ScheduleDayExtensions
	{ 
		public static void ModifyDictionary(this IScheduleDay scheduleDay)
		{
			scheduleDay.Owner.Modify(scheduleDay, new DoNothingScheduleDayChangeCallBack());
		}
	}
}