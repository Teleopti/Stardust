using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public class BlockPeriodFinderBetweenDayOff
    {
        public DateOnlyPeriod? GetBlockPeriod(IScheduleMatrixPro scheduleMatrixPro, DateOnly providedDateOnly)
        {
            //isSingleAgentTeam can be removed if more scheduling options are needed then we can move the scheduling options.
            if (scheduleMatrixPro == null) throw new ArgumentNullException(nameof(scheduleMatrixPro));
	        var person = scheduleMatrixPro.Person;
			var rangeForPerson = scheduleMatrixPro.ActiveScheduleRange;
	        
			var scheduleDay = rangeForPerson.ScheduledDay(providedDateOnly);
            if (isDayOff(scheduleDay))
		        return null;

			DateOnlyPeriod rangePeriod = rangeForPerson.Period.ToDateOnlyPeriod( TimeZoneGuard.Instance.CurrentTimeZone());
			var schedulePeriod = scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod;
			var personPeriod = person.Period(providedDateOnly);
			if (personPeriod == null)
				return providedDateOnly.ToDateOnlyPeriod();

	        var terminalDate = person.TerminalDate;
	        var personPeriodStartDate = personPeriod.StartDate;
			var startDate = traverse(rangeForPerson, rangePeriod, providedDateOnly, -1, schedulePeriod, personPeriodStartDate, terminalDate);
			var endDate = traverse(rangeForPerson, rangePeriod, providedDateOnly, 1, schedulePeriod, personPeriodStartDate, terminalDate);
	        if (endDate < startDate) return null;

	        return new DateOnlyPeriod(startDate, endDate);
        }

		private static DateOnly traverse(IScheduleRange rangeForPerson, DateOnlyPeriod rangePeriod, DateOnly providedDateOnly,
								  int stepDays, DateOnlyPeriod currentSchedulePeriod, DateOnly personPeriodStartDate, DateOnly? terminalDate)
		{
			var edgeDate = providedDateOnly;
			currentSchedulePeriod = new DateOnlyPeriod(currentSchedulePeriod.StartDate.AddDays(-10),
			                                           currentSchedulePeriod.EndDate.AddDays(10));
			
			while (rangePeriod.Contains(edgeDate) && currentSchedulePeriod.Contains(edgeDate) && edgeDate >= personPeriodStartDate)
			{
				if (terminalDate != null && edgeDate > terminalDate)
					return  terminalDate.Value;

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