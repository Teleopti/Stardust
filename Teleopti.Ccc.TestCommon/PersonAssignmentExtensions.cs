using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
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

		public static IPersonAssignment WithLayer(this IPersonAssignment assignment, IActivity activity, DateTimePeriod dateTimePeriod)
		{
			assignment.AddActivity(activity, dateTimePeriod);
			return assignment;
		}

		public static IPersonAssignment WithOvertimeLayer(this IPersonAssignment assignment, IActivity activity, TimePeriod period)
		{
			var assDate = assignment.Date.Date;
			var periodAsDateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(assDate.Add(period.StartTime), assDate.Add(period.EndTime), assignment.Person.PermissionInformation.DefaultTimeZone());
			assignment.AddOvertimeActivity(activity, periodAsDateTimePeriod, new MultiplicatorDefinitionSet("_", MultiplicatorType.Overtime));
			return assignment;
		}

		public static IPersonAssignment ShiftCategory(this IPersonAssignment assignment, IShiftCategory shiftCategory)
		{
			assignment.SetShiftCategory(shiftCategory);
			return assignment;
		}

		public static IPersonAssignment WithDayOff(this IPersonAssignment assignment)
		{
			assignment.SetDayOff(new DayOffTemplate());
			return assignment;
		}
	}
}