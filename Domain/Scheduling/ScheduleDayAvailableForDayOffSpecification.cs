using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ScheduleDayAvailableForDayOffSpecification : Specification<IScheduleDay>, IScheduleDayAvailableForDayOffSpecification
	{
		public override bool IsSatisfiedBy(IScheduleDay obj)
		{
			if (obj == null) return false;
			return obj.PersonAbsenceCollection().Count == 0 && obj.PersonAssignmentCollection().Count == 0 &&
			       obj.PersonDayOffCollection().Count == 0 && obj.PersonMeetingCollection().Count == 0;
		}
	}
}