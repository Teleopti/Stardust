using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
	public interface IVerifyWeeklyRestNotLockedAroundDayOffSpecification
    {
		bool IsSatisfy(DateOnly dayOff, IScheduleRange currentSchedules, IScheduleMatrixPro scheduleMatrix);
    }

	public class VerifyWeeklyRestNotLockedAroundDayOffSpecification : IVerifyWeeklyRestNotLockedAroundDayOffSpecification
    {
        public bool IsSatisfy(DateOnly dayOff, IScheduleRange currentSchedules, IScheduleMatrixPro scheduleMatrix)
        {
			var previousScheduleDay = currentSchedules.ScheduledDay(dayOff.AddDays(-1));
			var nextScheduleDay = currentSchedules.ScheduledDay(dayOff.AddDays(1));
            if (isNeighbouringDaysLocked(previousScheduleDay, nextScheduleDay, scheduleMatrix))
                return false;
            return true;
        }

		private bool isNeighbouringDaysLocked(IScheduleDay previousScheduleDay, IScheduleDay nextScheduleDay, IScheduleMatrixPro scheduleMatrixPro)
		{
			IList<DateTimePeriod> unlockeDays = scheduleMatrixPro.UnlockedDays.Select(day => day.DaySchedulePart().Period).ToList();
			if (!unlockeDays.Contains(previousScheduleDay.Period) || !unlockeDays.Contains(nextScheduleDay.Period))
				return true;
			return false;
		}
    }
}