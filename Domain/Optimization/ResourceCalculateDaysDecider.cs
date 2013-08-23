using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IResourceCalculateDaysDecider
    {
        IList<DateOnly> DecideDates(IScheduleDay currentSchedule, IScheduleDay previousSchedule);
    }

    public class ResourceCalculateDaysDecider : IResourceCalculateDaysDecider
    {
        public IList<DateOnly> DecideDates(IScheduleDay currentSchedule, IScheduleDay previousSchedule)
        {

            SchedulePartView current = currentSchedule.SignificantPart();
            SchedulePartView previous = previousSchedule.SignificantPart();
            DateOnly currentDate = currentSchedule.DateOnlyAsPeriod.DateOnly;

            if (!currentSchedule.IsScheduled() && previous == SchedulePartView.DayOff)
                return new List<DateOnly>();

            if(current == SchedulePartView.DayOff && previous == SchedulePartView.MainShift)
            {
                if(!isNightShift(previousSchedule))
                {
                    return new List<DateOnly> { currentDate };
                }
            }

            if (current == SchedulePartView.MainShift && previous == SchedulePartView.DayOff)
            {
                if (!isNightShift(currentSchedule))
                {
                    return new List<DateOnly> { currentDate };
                }
            }

            if(current == SchedulePartView.MainShift && previous == SchedulePartView.MainShift)
            {
                if (!isNightShift(previousSchedule) && !isNightShift(currentSchedule))
                {
                    return new List<DateOnly> { currentDate };
                }
            }

            if (!currentSchedule.IsScheduled() && previous == SchedulePartView.MainShift)
            {
                if (!isNightShift(previousSchedule))
                {
                    return new List<DateOnly> { currentDate };
                }
            }
            
            IList<DateOnly> ret = new List<DateOnly> {currentDate, currentDate.AddDays(1)};

            return ret;
        }

        private static bool isNightShift(IScheduleDay scheduleDay)
        {
            TimeZoneInfo tz = TeleoptiPrincipal.Current.Regional.TimeZone;
            DateOnly viewerStartDate = new DateOnly(scheduleDay.PersonAssignment().Period.StartDateTimeLocal(tz).Date);
            DateOnly viewerEndDate = new DateOnly(scheduleDay.PersonAssignment().Period.EndDateTimeLocal(tz).AddMinutes(-1).Date);

            return viewerStartDate != viewerEndDate;
        }
    }
}