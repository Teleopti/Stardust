using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class NightlyRestRule : IAssignmentPeriodRule
	{
		public DateTimePeriod LongestDateTimePeriodForAssignment(IScheduleRange current, DateOnly dateToCheck)
		{
			var timeZone = current.Person.PermissionInformation.DefaultTimeZone();
			var approximateTime = DateTime.SpecifyKind(dateToCheck.Date.AddHours(12), DateTimeKind.Unspecified);
			var approxUtc = TimeZoneHelper.ConvertToUtc(approximateTime, timeZone);

			var nightRest = findAndSetNightRestRule(current.Person, dateToCheck);
			if (!nightRest.HasValue)
				return new DateTimePeriod(approxUtc, approxUtc);

			IPersonAssignment assBefore = null;
			IPersonAssignment assAfter = null;

			var partCollection = current.ScheduledDayCollection(new DateOnlyPeriod(dateToCheck.AddDays(-3),
				dateToCheck.AddDays(3)));

			foreach (var scheduleDay in partCollection)
			{
				var ass = scheduleDay.PersonAssignment();
				if (ass == null || (ass.ShiftCategory == null && !ass.OvertimeActivities().Any())) continue;

				var assignmentPeriod = ass.Period;
				if (assignmentPeriod.Contains(approxUtc))
					return new DateTimePeriod(approxUtc, approxUtc);

				if (assignmentPeriod.EndDateTime < approxUtc)
				{
					assBefore = ass;
				}

				if (assAfter == null && approxUtc < assignmentPeriod.StartDateTime)
				{
					assAfter = ass;
				}
			}

			var earliestStartTime = endTimeOnAssignmentBeforePlusNightRest(current, assBefore, nightRest.Value);
			var latestEndTime = startTimeOnAssignmentAfterMinusNightRest(current, assAfter, nightRest.Value);

			if (latestEndTime < earliestStartTime)
			{
				latestEndTime = earliestStartTime;
			}
			return new DateTimePeriod(earliestStartTime, latestEndTime);
		}

		private static DateTime endTimeOnAssignmentBeforePlusNightRest(IScheduleRange currentCompleteRange,
			IPersonAssignment assBefore, TimeSpan nightRest)
		{
			return assBefore == null
				? currentCompleteRange.Period.StartDateTime
				: assBefore.Period.EndDateTime.AddHours(nightRest.TotalHours);
		}

		private static DateTime startTimeOnAssignmentAfterMinusNightRest(IScheduleRange currentCompleteRange,
			IPersonAssignment assAfter, TimeSpan nightRest)
		{
			return assAfter == null
				? currentCompleteRange.Period.EndDateTime
				: assAfter.Period.StartDateTime.AddHours(-nightRest.TotalHours);
		}

		private static TimeSpan? findAndSetNightRestRule(IPerson person, DateOnly dateOnly)
		{
			var period = person.Period(dateOnly);
			return period == null ? (TimeSpan?) null : period.PersonContract.Contract.WorkTimeDirective.NightlyRest;
		}
	}
}