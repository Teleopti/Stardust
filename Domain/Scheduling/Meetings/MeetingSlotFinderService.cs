using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.Scheduling.Meetings
{
    public interface IMeetingSlotFinderService
    {
        IList<TimePeriod> FindSlots(DateOnly dateOnly, TimeSpan duration, TimeSpan startTime, TimeSpan endTime,
                                                    IScheduleDictionary scheduleDictionary, IEnumerable<IPerson> persons);

        IList<DateOnly> FindAvailableDays(IList<DateOnly> dates, TimeSpan duration, TimeSpan startTime, TimeSpan endTime,
                                                          IScheduleDictionary scheduleDictionary, IEnumerable<IPerson> persons);
    }

    public class MeetingSlotFinderService : IMeetingSlotFinderService
    {
		private readonly IUserTimeZone _userTimeZone;
		private IEnumerable<IPerson> _personList = new List<IPerson>();

		public MeetingSlotFinderService(IUserTimeZone userTimeZone)
		{
			_userTimeZone = userTimeZone;
		}

		public IList<TimePeriod> FindSlots(DateOnly dateOnly, TimeSpan duration, TimeSpan startTime, TimeSpan endTime,
            IScheduleDictionary scheduleDictionary, IEnumerable<IPerson> persons)
        {
            InParameter.NotNull(nameof(scheduleDictionary), scheduleDictionary);

            IList<TimePeriod> timeList = new List<TimePeriod>();
            _personList = persons;
            var allAvailable = true;
            var localStartDateTime = TimeSpan.FromMinutes(0);
            var localEndDateTime = TimeSpan.FromMinutes(1440);
			
            HashSet<TimePeriod> notAllowedPeriods = new HashSet<TimePeriod>();

            foreach (var person in _personList)
            {
                var part = scheduleDictionary[person].ScheduledDay(dateOnly);

                if (part.HasDayOff())
                {
                    allAvailable = false;
                }
                else if (part.PersonAssignment() != null)
                {
                    var proj = part.ProjectionService().CreateProjection();
                    var absenceLayerList = proj.FilterLayers<IAbsence>();
                    foreach (IVisualLayer list in absenceLayerList)
                    {
                        AddNotAllowedPeriods(notAllowedPeriods,list, _userTimeZone);
                    }

                    // add layers with InWorkTime = false to list
                    foreach (var visualLayer in proj)
                    {
                        var activity = visualLayer.Payload as IActivity;
                        if(!((VisualLayer) visualLayer).HighestPriorityActivity.AllowOverwrite)
                            AddNotAllowedPeriods(notAllowedPeriods, visualLayer, _userTimeZone);
                        if (activity == null || activity.InWorkTime) continue;
                        AddNotAllowedPeriods(notAllowedPeriods, visualLayer, _userTimeZone);
                    }
               }

                localStartDateTime = GetLocalStartDateTime(part, _userTimeZone, localStartDateTime, ref localEndDateTime);
            }

            GetTimeList(notAllowedPeriods, localStartDateTime, localEndDateTime, allAvailable, endTime, startTime, duration, timeList);
            return timeList;
        }

        private static void AddNotAllowedPeriods(HashSet<TimePeriod> notAllowedPeriods, IVisualLayer visualLayer, IUserTimeZone userTimeZone)
        {
            var period = visualLayer.Period;
            notAllowedPeriods.Add(period.TimePeriod(userTimeZone.TimeZone()));
        }

        private static void GetTimeList(HashSet<TimePeriod> absencePeriods, TimeSpan localStartDateTime, TimeSpan localEndDateTime, bool allAvailable, TimeSpan endTime, TimeSpan startTime, TimeSpan duration, IList<TimePeriod> timeList)
        {
            if (allAvailable && (localStartDateTime < localEndDateTime))
            {
                var limit = (int)endTime.TotalMinutes;
                const int suggestionInterval = 30;
                if (startTime < localStartDateTime)
                {
                    startTime = TimeHelper.FitToDefaultResolution(localStartDateTime, 30);
                }
                if (endTime > localEndDateTime)
                {
                    if ((localEndDateTime.Minutes == 15) || (localEndDateTime.Minutes == 45))
                    {
                        endTime = TimeHelper.FitToDefaultResolutionRoundDown(localEndDateTime, 30);
                    }
                    else
                    {
                        endTime = TimeHelper.FitToDefaultResolution(localEndDateTime, 30);
                    }
                }
                SolveIntersection(endTime, startTime, limit, duration, absencePeriods, timeList, suggestionInterval);
            }
        }

        private static void SolveIntersection(TimeSpan end, TimeSpan start, int limit, TimeSpan duration, HashSet<TimePeriod> absencePeriods, IList<TimePeriod> timeList, int suggestionInterval)
        {
            var intersected = false;
            for (var t = (int)start.TotalMinutes; t <= limit; t += suggestionInterval)
            {
                var addTime = TimeSpan.FromMinutes(t);
                
                var addEndTime = addTime.Add(duration);
                var addDateTimePeriod = new TimePeriod(addTime, addEndTime);
                if (((addTime + duration) <= end) && (!absencePeriods.Contains(addDateTimePeriod)))
                {
                    foreach (var list in absencePeriods)
                    {
                        if (addDateTimePeriod.Intersect(list))
                        {
                            intersected = true;
                        }
                    }
                    if (!intersected)
                    {
                        timeList.Add(addDateTimePeriod);
                    }
                }
                intersected = false;
            }
        }

	    private static TimeSpan GetLocalStartDateTime(IScheduleDay part, IUserTimeZone userTimeZone, TimeSpan localStartDateTime,
		    ref TimeSpan localEndDateTime)
	    {
		    var assignment = part.PersonAssignment();
		    if (assignment == null)
			    return localStartDateTime;

		    var localPeriod = assignment.Period.TimePeriod(userTimeZone.TimeZone());
		    var localStartTime = localPeriod.StartTime;
		    if (localStartTime > localStartDateTime)
		    {
			    localStartDateTime = localStartTime;
		    }
		    var localEndTime = localPeriod.EndTime;
		    if (localEndTime < localEndDateTime)
		    {
			    localEndDateTime = localEndTime;
		    }

		    return localStartDateTime;
	    }

	    public IList<DateOnly> FindAvailableDays(IList<DateOnly> dates, TimeSpan duration, TimeSpan startTime, TimeSpan endTime,
            IScheduleDictionary scheduleDictionary, IEnumerable<IPerson> persons)
        {
            InParameter.NotNull(nameof(dates), dates);
            InParameter.NotNull(nameof(scheduleDictionary), scheduleDictionary);

            IList<DateOnly> availableDateList = new List<DateOnly>();

            foreach (var dateOnly in dates)
            {
                var timeList = FindSlots(dateOnly, duration, startTime, endTime, scheduleDictionary, persons);
                if (timeList.Count > 0)
                {
                    availableDateList.Add(dateOnly);
                }
            }
            return availableDateList;
        }
    }
}
