using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
	public interface IScheduleDayIsLockedSpecification
    {
		bool IsSatisfy(IScheduleDay day, IScheduleMatrixPro scheduleMatrix);
    }

	public class ScheduleDayIsLockedSpecification : IScheduleDayIsLockedSpecification
    {
		public bool IsSatisfy(IScheduleDay scheduleDay, IScheduleMatrixPro scheduleMatrix)
		{
			return isDayLocked(scheduleDay, scheduleMatrix);
		}

		private bool isDayLocked(IScheduleDay scheduleDay, IScheduleMatrixPro scheduleMatrixPro)
		{
			if (scheduleDay == null)
				return false;
			IList<DateTimePeriod> unlockeDays = scheduleMatrixPro.UnlockedDays.Select(day => day.DaySchedulePart().Period).ToList();
			if (!unlockeDays.Contains(scheduleDay.Period))
				return true;
			return false;
		}
    }
}