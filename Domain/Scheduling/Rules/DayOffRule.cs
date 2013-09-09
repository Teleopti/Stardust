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
            IDayOff dayOffBefore = null;
            IDayOff dayOffAfter = null;
            var  approximateTime = DateTime.SpecifyKind(dateToCheck.Date.AddHours(12), DateTimeKind.Unspecified);

			IEnumerable<IScheduleDay> partCollection = current.ScheduledDayCollection(new DateOnlyPeriod(dateToCheck.AddDays(-3),
		                                                                        dateToCheck.AddDays(3)));

		    foreach (IScheduleDay scheduleDay in partCollection)
		    {
			    var ass = scheduleDay.PersonAssignment();
					if (ass != null)
					{
						var dayOff = ass.DayOff();
						if (dayOff != null)
						{
							if (dayOff.Anchor < approximateTime)
							{
								dayOffBefore = dayOff;
							}
							if (dayOffAfter == null && approximateTime < dayOff.Anchor)
							{
								dayOffAfter = dayOff;
							}
						}
					}
		    }
            
            DateTime latestEndTime = latestEndTimeBeforeDayOff(dayOffAfter, partCollection);
            DateTime earliestStartTime = earliestStartTimeAfterDayOff(current, dayOffBefore, partCollection);

            return new DateTimePeriod(earliestStartTime, latestEndTime);
        }

		private static DateTime earliestStartTimeAfterDayOff(IScheduleRange current, IDayOff dayOff, IEnumerable<IScheduleDay> partCollection)
        {
            if (dayOff == null)
            {
                return current.Period.StartDateTime;
            }
            DateTime earliestStartTime;
            IPersonAssignment layerBefore = getAssignmentJustBeforeDayOff(dayOff, partCollection);
            if (layerBefore == null)
            {
                earliestStartTime = dayOff.InnerBoundary.EndDateTime;
            }
            else
            {
                TimeSpan differ = layerBefore.Period.EndDateTime - dayOffStartEnd(dayOff).StartDateTime;
                earliestStartTime = dayOffStartEnd(dayOff).EndDateTime.Add(differ);
                if(earliestStartTime < dayOff.InnerBoundary.EndDateTime)
                {
                    earliestStartTime = dayOff.InnerBoundary.EndDateTime;
                }
            }

            return earliestStartTime;
        }

		private static DateTime latestEndTimeBeforeDayOff(IDayOff dayOff, IEnumerable<IScheduleDay> partCollection)
        {
            if (dayOff == null)
            {
                return partCollection.Last().Period.EndDateTime;
            }
            DateTime latestEndTime;
            
            IPersonAssignment layerAfter = getAssignmentJustAfterDayOff(dayOff, partCollection);
            if (layerAfter == null)
            {
                latestEndTime = dayOff.InnerBoundary.StartDateTime;
            }
            else
            {
                TimeSpan differ = layerAfter.Period.StartDateTime - dayOffStartEnd(dayOff).EndDateTime;
                latestEndTime = dayOffStartEnd(dayOff).StartDateTime.Add(differ);
                if(latestEndTime > dayOff.InnerBoundary.StartDateTime)
                {
                    latestEndTime = dayOff.InnerBoundary.StartDateTime;
                }
            }

            return latestEndTime;
        }

        private static DateTimePeriod dayOffStartEnd(IDayOff dayOff)
        {
            DateTime startDayOff = dayOff.Anchor.AddMinutes(-(dayOff.TargetLength.TotalMinutes / 2));
            DateTime endDayOff = dayOff.Anchor.AddMinutes((dayOff.TargetLength.TotalMinutes / 2));

            return new DateTimePeriod(startDayOff, endDayOff);
        }

		private static IPersonAssignment getAssignmentJustBeforeDayOff(IDayOff dayOff, IEnumerable<IScheduleDay> partCollection)
        {
            IPersonAssignment returnVal = null;
            foreach (IScheduleDay scheduleDay in partCollection)
            {
	            var assignment = scheduleDay.PersonAssignment();
							if (assignment != null)
							{
								if (assignment.Period.StartDateTime.Date == dayOff.Anchor.Date)
									continue;
								if (assignment.Period.StartDateTime < dayOff.Anchor)
								{
									returnVal = assignment;
								}								
							}
            }
            
            return returnVal;
        }

		private static IPersonAssignment getAssignmentJustAfterDayOff(IDayOff dayOff, IEnumerable<IScheduleDay> partCollection)
        {
            foreach (IScheduleDay scheduleDay in partCollection)
            {
	            var assignment = scheduleDay.PersonAssignment();
							if (assignment != null)
							{
								if (assignment.Period.StartDateTime.Date == dayOff.Anchor.Date)
									continue;
								if (assignment.Period.StartDateTime > dayOff.Anchor)
									return assignment;								
							}
            }
            return null;
        }
    }
}
