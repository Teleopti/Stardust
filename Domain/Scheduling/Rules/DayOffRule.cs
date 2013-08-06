using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class DayOffRule :  IAssignmentPeriodRule
    {

		public DateTimePeriod LongestDateTimePeriodForAssignment(IScheduleRange current, DateOnly dateToCheck)
        {
            IPersonDayOff dayOffBefore = null;
            IPersonDayOff dayOffAfter = null;
            var  approximateTime = DateTime.SpecifyKind(dateToCheck.Date.AddHours(12), DateTimeKind.Unspecified);

			IEnumerable<IScheduleDay> partCollection = current.ScheduledDayCollection(new DateOnlyPeriod(dateToCheck.AddDays(-3),
		                                                                        dateToCheck.AddDays(3)));

		    foreach (IScheduleDay scheduleDay in partCollection)
		    {
                foreach (IPersonDayOff dayOff in scheduleDay.PersonDayOffCollection())
                {
                    if (dayOff.DayOff.Anchor < approximateTime)
                    {
                        dayOffBefore = dayOff;
                    }
                    if (dayOffAfter == null && approximateTime < dayOff.DayOff.Anchor)
                    {
                        dayOffAfter = dayOff;
                        break;
                    }
                }
		    }
            
            DateTime latestEndTime = latestEndTimeBeforeDayOff(dayOffAfter, partCollection);
            DateTime earliestStartTime = earliestStartTimeAfterDayOff(current, dayOffBefore, partCollection);

            return new DateTimePeriod(earliestStartTime, latestEndTime);
        }

		private static DateTime earliestStartTimeAfterDayOff(IScheduleRange current, IPersonDayOff dayOff, IEnumerable<IScheduleDay> partCollection)
        {
            if (dayOff == null)
            {
                return current.Period.StartDateTime;
            }
            DateTime earliestStartTime;
            IPersonAssignment layerBefore = getAssignmentJustBeforeDayOff(dayOff, partCollection);
            if (layerBefore == null)
            {
                earliestStartTime = dayOff.DayOff.InnerBoundary.EndDateTime;
            }
            else
            {
                TimeSpan differ = layerBefore.Period.EndDateTime - dayOffStartEnd(dayOff).StartDateTime;
                earliestStartTime = dayOffStartEnd(dayOff).EndDateTime.Add(differ);
                if(earliestStartTime < dayOff.DayOff.InnerBoundary.EndDateTime)
                {
                    earliestStartTime = dayOff.DayOff.InnerBoundary.EndDateTime;
                }
            }

            return earliestStartTime;
        }

		private static DateTime latestEndTimeBeforeDayOff(IPersonDayOff dayOff, IEnumerable<IScheduleDay> partCollection)
        {
            if (dayOff == null)
            {
                return partCollection.Last().Period.EndDateTime;
            }
            DateTime latestEndTime;
            
            IPersonAssignment layerAfter = getAssignmentJustAfterDayOff(dayOff, partCollection);
            if (layerAfter == null)
            {
                latestEndTime = dayOff.DayOff.InnerBoundary.StartDateTime;
            }
            else
            {
                TimeSpan differ = layerAfter.Period.StartDateTime - dayOffStartEnd(dayOff).EndDateTime;
                latestEndTime = dayOffStartEnd(dayOff).StartDateTime.Add(differ);
                if(latestEndTime > dayOff.DayOff.InnerBoundary.StartDateTime)
                {
                    latestEndTime = dayOff.DayOff.InnerBoundary.StartDateTime;
                }
            }

            return latestEndTime;
        }

        private static DateTimePeriod dayOffStartEnd(IPersonDayOff dayOff)
        {
            DateTime startDayOff = dayOff.DayOff.Anchor.AddMinutes(-(dayOff.DayOff.TargetLength.TotalMinutes / 2));
            DateTime endDayOff = dayOff.DayOff.Anchor.AddMinutes((dayOff.DayOff.TargetLength.TotalMinutes / 2));

            return new DateTimePeriod(startDayOff, endDayOff);
        }

		private static IPersonAssignment getAssignmentJustBeforeDayOff(IPersonDayOff dayOff, IEnumerable<IScheduleDay> partCollection)
        {
            IPersonAssignment returnVal = null;
            foreach (IScheduleDay scheduleDay in partCollection)
            {
                foreach (var assignment in scheduleDay.PersonAssignmentCollectionDoNotUse())
                {
                    if(assignment.Period.StartDateTime.Date == dayOff.DayOff.Anchor.Date)
                        continue;
                    if (assignment.Period.StartDateTime < dayOff.DayOff.Anchor)
                    {
                        returnVal = assignment;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            
            return returnVal;
        }

		private static IPersonAssignment getAssignmentJustAfterDayOff(IPersonDayOff dayOff, IEnumerable<IScheduleDay> partCollection)
        {
            foreach (IScheduleDay scheduleDay in partCollection)
            {
                foreach (var assignment in scheduleDay.PersonAssignmentCollectionDoNotUse())
                {
                    if (assignment.Period.StartDateTime.Date == dayOff.DayOff.Anchor.Date)
                        continue;
                    if (assignment.Period.StartDateTime > dayOff.DayOff.Anchor)
                        return assignment;
                }
            }
            return null;
        }
    }
}
