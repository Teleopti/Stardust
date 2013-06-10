using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    public class OverlappingAssignmentRule : IAssignmentPeriodRule
    {
        public DateTimePeriod LongestDateTimePeriodForAssignment(IScheduleRange current, DateOnly dateToCheck)
        {
            var timeZone = current.Person.PermissionInformation.DefaultTimeZone();
            IPersonAssignment assBefore = null;
            IPersonAssignment assAfter = null;
            var approximateTime = DateTime.SpecifyKind(dateToCheck.Date.AddHours(12), DateTimeKind.Unspecified);
            DateTime approxUtc = TimeZoneHelper.ConvertToUtc(approximateTime, timeZone);
			var partCollection = current.ScheduledDayCollection(new DateOnlyPeriod(dateToCheck.AddDays(-3),
																				dateToCheck.AddDays(3)));
			foreach (IScheduleDay scheduleDay in partCollection)
            {
                foreach (IPersonAssignment ass in scheduleDay.PersonAssignmentCollection())
                {
                    if (ass.ShiftCategory != null)
                    {
                        if (ass.Period.StartDateTime <= approxUtc && ass.Period.EndDateTime >= approxUtc)
                        {
                            return new DateTimePeriod(approxUtc, approxUtc);
                        }
                        if (ass.Period.EndDateTime < approxUtc)
                        {
                            assBefore = ass;
                        }
                        if (approxUtc < ass.Period.StartDateTime)
                        {
                            assAfter = ass;
                            break;
                        }
                    }
                }
            }
            
            DateTime earliestStartTime = endTimeOnAssignmentBefore(current, assBefore);
            DateTime latestEndTime = startTimeOnAssignmentAfter(current, assAfter);

            if (latestEndTime < earliestStartTime)
            {
                latestEndTime = earliestStartTime;
            }
            return new DateTimePeriod(earliestStartTime, latestEndTime);
        }

        private static DateTime endTimeOnAssignmentBefore(IScheduleRange currentCompleteRange, IPersonAssignment assBefore)
        {
            return assBefore == null ? currentCompleteRange.Period.StartDateTime : assBefore.Period.EndDateTime;
        }

        private static DateTime startTimeOnAssignmentAfter(IScheduleRange currentCompleteRange, IPersonAssignment assAfter)
        {
            return assAfter == null ? currentCompleteRange.Period.EndDateTime : assAfter.Period.StartDateTime;
        }
    }
}

