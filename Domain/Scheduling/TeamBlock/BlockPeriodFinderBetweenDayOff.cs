using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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

			DateOnlyPeriod rangePeriod = rangeForPerson.Period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());
			var schedulePeriod = scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod;
			var personStartDate = DateOnly.MaxValue;
			foreach (var personPeriod in person.PersonPeriodCollection)
			{
				personStartDate = personPeriod.StartDate < personStartDate ? personPeriod.StartDate : personStartDate;
			}
			var terminalDate = person.TerminalDate;
			var startDate = traverse(rangeForPerson, rangePeriod, providedDateOnly, -1, schedulePeriod, terminalDate, personStartDate);
			var endDate = traverse(rangeForPerson, rangePeriod, providedDateOnly, 1, schedulePeriod, terminalDate, personStartDate);
	        if (endDate < startDate) return null;

	        return new DateOnlyPeriod(startDate, endDate);
        }

		private static DateOnly traverse(IScheduleRange rangeForPerson, DateOnlyPeriod rangePeriod, DateOnly providedDateOnly,
								  int stepDays, DateOnlyPeriod currentSchedulePeriod, DateOnly? terminalDate, DateOnly personStartDate)
		{
			var edgeDate = providedDateOnly;
			currentSchedulePeriod = new DateOnlyPeriod(currentSchedulePeriod.StartDate.AddDays(-10),
			                                           currentSchedulePeriod.EndDate.AddDays(10));
			
			while (rangePeriod.Contains(edgeDate) && currentSchedulePeriod.Contains(edgeDate))
			{
				if (edgeDate < personStartDate)
					return personStartDate;
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