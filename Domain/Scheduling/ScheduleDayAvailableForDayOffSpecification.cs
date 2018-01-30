using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ScheduleDayAvailableForDayOffSpecification : Specification<IScheduleDay>, IScheduleDayAvailableForDayOffSpecification
	{
		public override bool IsSatisfiedBy(IScheduleDay obj)
		{
			if (obj == null) 
				return false;

			return !obj.IsScheduled() && obj.PersonMeetingCollection().Length == 0 && obj.PersonAbsenceCollection().Length == 0;
		}
	}
}