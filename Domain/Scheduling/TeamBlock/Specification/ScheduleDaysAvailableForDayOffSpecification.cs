using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification
{
	public interface IScheduleDaysAvailableForDayOffSpecification
	{
		bool IsSatisfiedBy(IList<IScheduleDay> scheduleDays);
	}

	public class ScheduleDaysAvailableForDayOffSpecification : IScheduleDaysAvailableForDayOffSpecification
	{
		public bool IsSatisfiedBy(IList<IScheduleDay> scheduleDays)
		{
			if (scheduleDays == null) return false;

			foreach (var scheduleDay in scheduleDays)
			{
				var result = scheduleDay.PersonAbsenceCollection().Count == 0 && !scheduleDay.IsScheduled() &&
				              scheduleDay.PersonMeetingCollection().Count == 0;
				if (!result)
					return false;
			}
			return true;
		}
	}
}