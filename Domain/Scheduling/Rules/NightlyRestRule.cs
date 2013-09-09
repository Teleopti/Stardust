using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    public class NightlyRestRule : IAssignmentPeriodRule
    {
        public DateTimePeriod LongestDateTimePeriodForAssignment(IScheduleRange current, DateOnly dateToCheck)
        {
            var timeZone = current.Person.PermissionInformation.DefaultTimeZone();
            var approximateTime = DateTime.SpecifyKind(dateToCheck.Date.AddHours(12), DateTimeKind.Unspecified);
            DateTime approxUtc = TimeZoneHelper.ConvertToUtc(approximateTime, timeZone);

        	TimeSpan? nightRest = findAndSetNightRestRule(current.Person, dateToCheck);
            if (!nightRest.HasValue)
                return new DateTimePeriod(approxUtc, approxUtc);

            IPersonAssignment assBefore = null;
            IPersonAssignment assAfter = null;

            //var schedules = current.ScheduledDayCollection(current.Period.ToDateOnlyPeriod(timeZone));
			var partCollection = current.ScheduledDayCollection(new DateOnlyPeriod(dateToCheck.AddDays(-3),
																				dateToCheck.AddDays(3)));

			foreach (IScheduleDay scheduleDay in partCollection)
			{
				var ass = scheduleDay.PersonAssignment();
				if (ass != null && ass.ShiftCategory!=null)
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
					}
				}
            }
            
            DateTime earliestStartTime = endTimeOnAssignmentBeforePlusNightRest(current, assBefore, nightRest.Value);
			DateTime latestEndTime = startTimeOnAssignmentAfterMinusNightRest(current, assAfter, nightRest.Value);

            if (latestEndTime < earliestStartTime)
            {
                latestEndTime = earliestStartTime;
            }
            return new DateTimePeriod(earliestStartTime, latestEndTime);
        }

        private static DateTime endTimeOnAssignmentBeforePlusNightRest(IScheduleRange currentCompleteRange, IPersonAssignment assBefore, TimeSpan nightRest)
        {
            if (assBefore == null)
            {
                return currentCompleteRange.Period.StartDateTime;
            }
            return assBefore.Period.EndDateTime.AddHours(nightRest.TotalHours);
        }

		private static DateTime startTimeOnAssignmentAfterMinusNightRest(IScheduleRange currentCompleteRange, IPersonAssignment assAfter, TimeSpan nightRest)
        {
            if (assAfter == null)
            {
                return currentCompleteRange.Period.EndDateTime;
            }
            return assAfter.Period.StartDateTime.AddHours(- nightRest.TotalHours);
        }

        private static TimeSpan? findAndSetNightRestRule(IPerson person, DateOnly dateOnly)
        {
            IPersonPeriod period = person.Period(dateOnly);
            if (period == null)
            {
                return null;
            }
            return period.PersonContract.Contract.WorkTimeDirective.NightlyRest;
        }
    }
}
