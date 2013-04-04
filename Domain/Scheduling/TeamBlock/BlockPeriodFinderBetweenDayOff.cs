using System;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface IBlockPeriodFinderBetweenDayOff
    {
		DateOnlyPeriod? GetBlockPeriod(IScheduleMatrixPro scheduleMatrixPro, DateOnly providedDateOnly);
    }

    public class BlockPeriodFinderBetweenDayOff : IBlockPeriodFinderBetweenDayOff
    {
        public DateOnlyPeriod? GetBlockPeriod(IScheduleMatrixPro scheduleMatrixPro, DateOnly providedDateOnly)
        {
            if (scheduleMatrixPro == null) throw new ArgumentNullException("scheduleMatrixPro");
            IScheduleRange rangeForPerson = scheduleMatrixPro.SchedulingStateHolder.Schedules[scheduleMatrixPro.Person];
	        
			IScheduleDay scheduleDay = rangeForPerson.ScheduledDay(providedDateOnly);
	        if (isDayOff(scheduleDay))
		        return null;

			DateOnlyPeriod rangePeriod = rangeForPerson.Period.ToDateOnlyPeriod(TeleoptiPrincipal.Current.Regional.TimeZone);
			var schedulePeriod = scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod;

	        DateOnly startDate = traverse(rangeForPerson, rangePeriod, providedDateOnly.AddDays(-1), -1,schedulePeriod);
			DateOnly endDate = traverse(rangeForPerson, rangePeriod, providedDateOnly.AddDays(1), 1, schedulePeriod);

	        return new DateOnlyPeriod(startDate, endDate);
        }

		private static DateOnly traverse(IScheduleRange rangeForPerson, DateOnlyPeriod rangePeriod, DateOnly providedDateOnly,
								  int stepDays, DateOnlyPeriod currentSchedulePeriod)
		{
			DateOnly edgeDate = providedDateOnly;
			currentSchedulePeriod = new DateOnlyPeriod(currentSchedulePeriod.StartDate.AddDays(-10),
			                                           currentSchedulePeriod.EndDate.AddDays(10));
			while (rangePeriod.Contains(edgeDate) && currentSchedulePeriod.Contains(edgeDate))
			{
				IScheduleDay scheduleDay = rangeForPerson.ScheduledDay(edgeDate);
				if (isDayOff(scheduleDay))
					break;

				edgeDate = edgeDate.AddDays(stepDays);
			}

			return edgeDate.AddDays(-stepDays);
		}

        //Absence can not be a block breaker when using teams
        private static bool isDayOff(IScheduleDay scheduleDay)
        {
            var significantPart = scheduleDay.SignificantPart();
            if (significantPart == SchedulePartView.DayOff ||
                significantPart == SchedulePartView.ContractDayOff)
            {
                return true;
            }
            return false;
        }
    }
}