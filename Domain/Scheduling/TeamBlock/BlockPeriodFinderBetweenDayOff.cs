using System;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface IBlockPeriodFinderBetweenDayOff
    {
        DateOnlyPeriod? GetBlockPeriod(IScheduleMatrixPro scheduleMatrixPro, DateOnly providedDateOnly, bool isSingleAgentTeam);
    }

    public class BlockPeriodFinderBetweenDayOff : IBlockPeriodFinderBetweenDayOff
    {
        public DateOnlyPeriod? GetBlockPeriod(IScheduleMatrixPro scheduleMatrixPro, DateOnly providedDateOnly,bool isSingleAgentTeam)
        {
            //isSingleAgentTeam can be removed if more scheduling options are needed then we can move the scheduling options.
            if (scheduleMatrixPro == null) throw new ArgumentNullException("scheduleMatrixPro");
	        var person = scheduleMatrixPro.Person;
			IScheduleRange rangeForPerson = scheduleMatrixPro.SchedulingStateHolder.Schedules[person];
	        
			IScheduleDay scheduleDay = rangeForPerson.ScheduledDay(providedDateOnly);
            if (isDayOff(scheduleDay))
		        return null;

			DateOnlyPeriod rangePeriod = rangeForPerson.Period.ToDateOnlyPeriod(TeleoptiPrincipal.Current.Regional.TimeZone);
			var schedulePeriod = scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod;
			var personPeriod = person.Period(providedDateOnly);
			if (personPeriod == null)
				return new DateOnlyPeriod(providedDateOnly, providedDateOnly);
	        DateOnly personPeriodStartDate = personPeriod.StartDate;
			DateOnly startDate = traverse(rangeForPerson, rangePeriod, providedDateOnly, -1, schedulePeriod, isSingleAgentTeam, personPeriodStartDate);
			DateOnly endDate = traverse(rangeForPerson, rangePeriod, providedDateOnly, 1, schedulePeriod, isSingleAgentTeam, personPeriodStartDate);

	        return new DateOnlyPeriod(startDate, endDate);
        }

		private static DateOnly traverse(IScheduleRange rangeForPerson, DateOnlyPeriod rangePeriod, DateOnly providedDateOnly,
								  int stepDays, DateOnlyPeriod currentSchedulePeriod, bool isSingleAgentTeam, DateOnly personPeriodStartDate)
		{
			DateOnly edgeDate = providedDateOnly;
			currentSchedulePeriod = new DateOnlyPeriod(currentSchedulePeriod.StartDate.AddDays(-10),
			                                           currentSchedulePeriod.EndDate.AddDays(10));
			
			while (rangePeriod.Contains(edgeDate) && currentSchedulePeriod.Contains(edgeDate) && edgeDate >= personPeriodStartDate)
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