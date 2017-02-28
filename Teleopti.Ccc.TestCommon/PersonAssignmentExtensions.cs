using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public static class PersonAssignmentExtensions
	{
		public static IPersonAssignment WithLayer(this IPersonAssignment assignment, IActivity activity, TimePeriod period)
		{
			assignment.AddActivity(activity, period);
			return assignment;
		}

		public static IPersonAssignment ShiftCategory(this IPersonAssignment assignment, IShiftCategory shiftCategory)
		{
			assignment.SetShiftCategory(shiftCategory);
			return assignment;
		}

		public static IPersonAssignment IsDayOff(this IPersonAssignment assignment)
		{
			assignment.SetDayOff(new DayOffTemplate());
			return assignment;
		}
	}
}