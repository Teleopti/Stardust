using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public static class ScheduleDayExtensions
	{
		public static void Replace(this IScheduleDay scheduleDay, IPersonAssignment personAssignment)
		{
			scheduleDay.Remove(personAssignment);
			scheduleDay.Add(personAssignment);
		}
	}
}