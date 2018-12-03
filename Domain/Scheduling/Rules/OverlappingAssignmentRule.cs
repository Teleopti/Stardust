using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
			var approxUtc = TimeZoneHelper.ConvertToUtc(approximateTime, timeZone);
			var partCollection = current.ScheduledDayCollection(new DateOnlyPeriod(dateToCheck.AddDays(-3),
																				dateToCheck.AddDays(3)));
			foreach (var scheduleDay in partCollection)
			{
				var ass = scheduleDay.PersonAssignment();
				if (ass?.ShiftCategory == null) continue;

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
				}
			}

			var earliestStartTime = endTimeOnAssignmentBefore(current, assBefore);
			var latestEndTime = startTimeOnAssignmentAfter(current, assAfter);

			if (latestEndTime < earliestStartTime)
			{
				latestEndTime = earliestStartTime;
			}
			return new DateTimePeriod(earliestStartTime, latestEndTime);
		}

		private static DateTime endTimeOnAssignmentBefore(IScheduleRange currentCompleteRange, IPersonAssignment assBefore)
		{
			return assBefore?.Period.EndDateTime ?? currentCompleteRange.Period.StartDateTime;
		}

		private static DateTime startTimeOnAssignmentAfter(IScheduleRange currentCompleteRange, IPersonAssignment assAfter)
		{
			return assAfter?.Period.StartDateTime ?? currentCompleteRange.Period.EndDateTime;
		}
	}
}