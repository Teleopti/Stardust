using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

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