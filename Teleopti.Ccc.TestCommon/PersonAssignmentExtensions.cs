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
	}
}