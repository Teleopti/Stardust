using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    public class NightlyRestRule : IAssignmentPeriodRule
    {
        private TimeSpan _nightlyRest;

        public DateTimePeriod LongestDateTimePeriodForAssignment(IScheduleRange current, DateOnly dateToCheck)
        {
            var timeZone = current.Person.PermissionInformation.DefaultTimeZone();
            var approximateTime = DateTime.SpecifyKind(dateToCheck.Date.AddHours(12), DateTimeKind.Unspecified);
            DateTime approxUtc = TimeZoneHelper.ConvertToUtc(approximateTime, timeZone);

            if (FindAndSetNightRestRule(current.Person, dateToCheck) == false)
                return new DateTimePeriod(approxUtc, approxUtc);

            IPersonAssignment assBefore = null;
            IPersonAssignment assAfter = null;

            var schedules = current.ScheduledDayCollection(current.Period.ToDateOnlyPeriod(timeZone));
            foreach (IScheduleDay scheduleDay in schedules)
            {
                foreach (IPersonAssignment ass in scheduleDay.PersonAssignmentCollection())
                {
                    if (ass.MainShift != null)
                    {
                        if (ass.Period.Contains(approxUtc))
                            return new DateTimePeriod(approxUtc, approxUtc);

                        if (ass.Period.EndDateTime < approxUtc)
                        {
                            assBefore = ass;
                        }
                        if (assAfter == null && approxUtc < ass.Period.StartDateTime)
                        {
                            assAfter = ass;
                            break;
                        }
                    }
                }
            }
            
            DateTime earliestStartTime = endTimeOnAssignmentBeforePlusNightRest(current, assBefore);
            DateTime latestEndTime = startTimeOnAssignmentAfterMinusNightRest(current, assAfter);

            if (latestEndTime < earliestStartTime)
            {
                latestEndTime = earliestStartTime;
            }
            return new DateTimePeriod(earliestStartTime, latestEndTime);
        }

        private DateTime endTimeOnAssignmentBeforePlusNightRest(IScheduleRange currentCompleteRange, IPersonAssignment assBefore)
        {
            if (assBefore == null)
            {
                return currentCompleteRange.Period.StartDateTime;
            }
            return assBefore.Period.EndDateTime.AddHours(_nightlyRest.TotalHours);
        }

        private DateTime startTimeOnAssignmentAfterMinusNightRest(IScheduleRange currentCompleteRange, IPersonAssignment assAfter)
        {
            if (assAfter == null)
            {
                return currentCompleteRange.Period.EndDateTime;
            }
            return assAfter.Period.StartDateTime.AddHours(-_nightlyRest.TotalHours);
        }

        private bool FindAndSetNightRestRule(IPerson person, DateOnly dateOnly)
        {
            IPersonPeriod period = person.Period(dateOnly);
            if (period == null)
            {
                return false;
            }
            _nightlyRest = period.PersonContract.Contract.WorkTimeDirective.NightlyRest;
            return true;
        }
    }
}
