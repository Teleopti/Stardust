using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class DayOffRule :  IAssignmentPeriodRule
	{
		public DateTimePeriod LongestDateTimePeriodForAssignment(IScheduleRange current, DateOnly dateToCheck)
		{
			DayOff dayOffBefore = null;
			DayOff dayOffAfter = null;
			var  approximateTime = DateTime.SpecifyKind(dateToCheck.Date.AddHours(12), DateTimeKind.Unspecified);

			var period = new DateOnlyPeriod(dateToCheck.AddDays(-3), dateToCheck.AddDays(3));
			var partCollection = current.ScheduledDayCollection(period).ToList();

			foreach (var scheduleDay in partCollection)
			{
				var ass = scheduleDay.PersonAssignment();
				var dayOff = ass?.DayOff();
				if (dayOff == null) continue;

				if (dayOff.Anchor < approximateTime)
				{
					dayOffBefore = dayOff;
				}
				if (dayOffAfter == null && approximateTime < dayOff.Anchor)
				{
					dayOffAfter = dayOff;
				}
			}

			var latestEndTime = latestEndTimeBeforeDayOff(dayOffAfter, partCollection);
			var earliestStartTime = earliestStartTimeAfterDayOff(current, dayOffBefore, partCollection);

			return new DateTimePeriod(earliestStartTime, latestEndTime);
		}

		private static DateTime earliestStartTimeAfterDayOff(IScheduleRange current, DayOff dayOff, IEnumerable<IScheduleDay> partCollection)
		{
			if (dayOff == null)
			{
				return current.Period.StartDateTime;
			}
			DateTime earliestStartTime;
			var layerBefore = getScheduledAssignmentJustBeforeDayOff(dayOff, partCollection);
			if (layerBefore == null)
			{
				earliestStartTime = dayOff.InnerBoundary.EndDateTime;
			}
			else
			{
				var differ = layerBefore.Period.EndDateTime - dayOffStartEnd(dayOff).StartDateTime;
				earliestStartTime = dayOffStartEnd(dayOff).EndDateTime.Add(differ);
				if(earliestStartTime < dayOff.InnerBoundary.EndDateTime)
				{
					earliestStartTime = dayOff.InnerBoundary.EndDateTime;
				}
			}

			return earliestStartTime;
		}

		private static DateTime latestEndTimeBeforeDayOff(DayOff dayOff, IEnumerable<IScheduleDay> partCollection)
		{
			if (dayOff == null)
			{
				return partCollection.Last().Period.EndDateTime;
			}
			DateTime latestEndTime;

			var layerAfter = getScheduledAssignmentJustAfterDayOff(dayOff, partCollection);
			if (layerAfter == null)
			{
				latestEndTime = dayOff.InnerBoundary.StartDateTime;
			}
			else
			{
				var differ = layerAfter.Period.StartDateTime - dayOffStartEnd(dayOff).EndDateTime;
				latestEndTime = dayOffStartEnd(dayOff).StartDateTime.Add(differ);
				if(latestEndTime > dayOff.InnerBoundary.StartDateTime)
				{
					latestEndTime = dayOff.InnerBoundary.StartDateTime;
				}
			}

			return latestEndTime;
		}

		private static DateTimePeriod dayOffStartEnd(DayOff dayOff)
		{
			var startDayOff = dayOff.Anchor.AddMinutes(-(dayOff.TargetLength.TotalMinutes / 2));
			var endDayOff = dayOff.Anchor.AddMinutes((dayOff.TargetLength.TotalMinutes / 2));

			return new DateTimePeriod(startDayOff, endDayOff);
		}

		private static IPersonAssignment getScheduledAssignmentJustBeforeDayOff(DayOff dayOff, IEnumerable<IScheduleDay> partCollection)
		{
			IPersonAssignment returnVal = null;
			foreach (var scheduleDay in partCollection)
			{
				var assignment = scheduleDay.PersonAssignment();
				if (assignment == null || !assignment.MainActivities().Any()) continue;
				if (assignment.Period.StartDateTime.Date == dayOff.Anchor.Date)
					continue;
				if (assignment.Period.StartDateTime < dayOff.Anchor)
				{
					returnVal = assignment;
				}
			}

			return returnVal;
		}

		private static IPersonAssignment getScheduledAssignmentJustAfterDayOff(DayOff dayOff,
			IEnumerable<IScheduleDay> partCollection)
		{
			var assignments = partCollection.Select(scheduleDay => scheduleDay.PersonAssignment());
			var validAssignments = assignments.Where(assignment => assignment != null && assignment.MainActivities().Any())
				.Where(assignment => assignment.Period.StartDateTime.Date != dayOff.Anchor.Date);

			return validAssignments.FirstOrDefault(assignment => assignment.Period.StartDateTime > dayOff.Anchor);
		}
	}
}