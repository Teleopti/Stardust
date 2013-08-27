using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface IScheduleDayAvailableForDayOffSpecification
	{
		bool IsSatisfiedBy(IScheduleDay part);
	}

	public class ScheduleDayAvailableForDayOffSpecification : Specification<IScheduleDay>, IScheduleDayAvailableForDayOffSpecification
	{
		public override bool IsSatisfiedBy(IScheduleDay obj)
		{
			if (obj == null) return false;
			return obj.PersonAbsenceCollection().Count == 0 && obj.PersonAssignment()==null && obj.PersonMeetingCollection().Count == 0;
		}
	}
}