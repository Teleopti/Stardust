using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class DayOffRule :  IAssignmentPeriodRule
    {
        private IEnumerable<IScheduleDay> _partCollection;
       
		public DateTimePeriod LongestDateTimePeriodForAssignment(IScheduleRange current, DateOnly dateToCheck)
        {
            IPersonDayOff dayOffBefore = null;
            IPersonDayOff dayOffAfter = null;
            var  approximateTime = DateTime.SpecifyKind(dateToCheck.Date.AddHours(12), DateTimeKind.Unspecified);
            
		    _partCollection = current.ScheduledDayCollection(new DateOnlyPeriod(dateToCheck.AddDays(-3),
		                                                                        dateToCheck.AddDays(3)));

		    foreach (IScheduleDay scheduleDay in _partCollection)
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
            
            DateTime latestEndTime = latestEndTimeBeforeDayOff(dayOffAfter);
            DateTime earliestStartTime = earliestStartTimeAfterDayOff(current, dayOffBefore);

            return new DateTimePeriod(earliestStartTime, latestEndTime);
        }
        
        private DateTime earliestStartTimeAfterDayOff(IScheduleRange current, IPersonDayOff dayOff)
        {
            if (dayOff == null)
            {
                return current.Period.StartDateTime;
            }
            DateTime earliestStartTime;
            IPersonAssignment layerBefore = getAssignmentJustBeforeDayOff(dayOff);
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

        private DateTime latestEndTimeBeforeDayOff(IPersonDayOff dayOff)
        {
            if (dayOff == null)
            {
                return _partCollection.Last().Period.EndDateTime;
            }
            DateTime latestEndTime;
            
            IPersonAssignment layerAfter = getAssignmentJustAfterDayOff(dayOff);
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

        private IPersonAssignment getAssignmentJustBeforeDayOff(IPersonDayOff dayOff)
        {
            IPersonAssignment returnVal = null;
            foreach (IScheduleDay scheduleDay in _partCollection)
            {
                foreach (var assignment in scheduleDay.PersonAssignmentCollection())
                {
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

        private IPersonAssignment getAssignmentJustAfterDayOff(IPersonDayOff dayOff)
        {
            foreach (IScheduleDay scheduleDay in _partCollection)
            {
                foreach (var assignment in scheduleDay.PersonAssignmentCollection())
                {
                    if (assignment.Period.StartDateTime > dayOff.DayOff.Anchor)
                        return assignment;
                }
            }
            return null;
        }
    }
}
