﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

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
        private IEnumerable<IPerson> _personList = new List<IPerson>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public IList<TimePeriod> FindSlots(DateOnly dateOnly, TimeSpan duration, TimeSpan startTime, TimeSpan endTime,
            IScheduleDictionary scheduleDictionary, IEnumerable<IPerson> persons)
        {
            InParameter.NotNull("dateOnly", dateOnly);
            InParameter.NotNull("scheduleDictionary", scheduleDictionary);

            IList<TimePeriod> timeList = new List<TimePeriod>();
            _personList = persons;
            var allAvailable = true;
            var localStartDateTime = TimeSpan.FromMinutes(0);
            var localEndDateTime = TimeSpan.FromMinutes(1440);

            IList<TimePeriod> notAllowedPeriods = new List<TimePeriod>();

            foreach (var person in _personList)
            {
                var part = scheduleDictionary[person].ScheduledDay(dateOnly);

                if (part.HasDayOff())
                {
                    allAvailable = false;
                }
                else if (part.PersonAssignmentCollectionDoNotUse().Count > 0)
                {
                    var proj = part.ProjectionService().CreateProjection();
                    var absenceLayerList = proj.FilterLayers<IAbsence>();
                    foreach (IVisualLayer list in absenceLayerList)
                    {
                        var startAbsence = list.Period.LocalStartDateTime.TimeOfDay;
                        var endAbsence = list.Period.LocalEndDateTime.TimeOfDay;
                        if (endAbsence < startAbsence) endAbsence = endAbsence.Add(TimeSpan.FromHours(24));
                        notAllowedPeriods.Add(new TimePeriod(startAbsence, endAbsence));
                    }

                    // add layers with InWorkTime = false to list
                    foreach (var visualLayer in proj)
                    {
                        var activity = visualLayer.Payload as IActivity;
                        if(!(((VisualLayer) visualLayer).HighestPriorityActivity.AllowOverwrite  ))
                            AddNotAllowedPeriods(notAllowedPeriods, visualLayer);
                        if (activity == null || activity.InWorkTime) continue;
                        AddNotAllowedPeriods(notAllowedPeriods, visualLayer);
                    }
               }

                localStartDateTime = GetLocalStartDateTime(part, localStartDateTime, ref localEndDateTime);
            }

            GetTimeList(notAllowedPeriods, localStartDateTime, localEndDateTime, allAvailable, endTime, startTime, duration, timeList);
            return timeList;
        }

        private static void AddNotAllowedPeriods(IList<TimePeriod> notAllowedPeriods, IVisualLayer visualLayer)
        {
            var period = visualLayer.Period;
            var startPeriod = period.LocalStartDateTime.TimeOfDay;
            var endPeriod = period.LocalEndDateTime.TimeOfDay;
            if (endPeriod < startPeriod) endPeriod = endPeriod.Add(TimeSpan.FromHours(24));
            notAllowedPeriods.Add(new TimePeriod(startPeriod, endPeriod));
        }

        private static void GetTimeList(IList<TimePeriod> absencePeriods, TimeSpan localStartDateTime, TimeSpan localEndDateTime, bool allAvailable, TimeSpan endTime, TimeSpan startTime, TimeSpan duration, IList<TimePeriod> timeList)
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

        private static void SolveIntersection(TimeSpan end, TimeSpan start, int limit, TimeSpan duration, IList<TimePeriod> absencePeriods, IList<TimePeriod> timeList, int suggestionInterval)
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

        private static TimeSpan GetLocalStartDateTime(IScheduleDay part, TimeSpan localStartDateTime, ref TimeSpan localEndDateTime)
        {
            var scheduleStart = part.PersonAssignmentCollectionDoNotUse();
            if (scheduleStart.Count <= 0) return localStartDateTime;
            foreach (IPersonAssignment t in scheduleStart)
            {
                var period = t.Period;
                var localStartTime = period.LocalStartDateTime.TimeOfDay;
                if (localStartTime > localStartDateTime)
                {
                    localStartDateTime = localStartTime;
                }
                var localEndTime = period.LocalEndDateTime.TimeOfDay;

                var startDateTime = period.StartDateTime.Date;
                var endDateTime = period.EndDateTime.Date;

                if (startDateTime != endDateTime)
                {
                    localEndTime = new TimeSpan(23, 59, 0);
                }

                if (localEndTime < localEndDateTime)
                {
                    localEndDateTime = localEndTime;
                }
            }
            return localStartDateTime;
        }

        public IList<DateOnly> FindAvailableDays(IList<DateOnly> dates, TimeSpan duration, TimeSpan startTime, TimeSpan endTime,
            IScheduleDictionary scheduleDictionary, IEnumerable<IPerson> persons)
        {
            InParameter.NotNull("dates", dates);
            InParameter.NotNull("scheduleDictionary", scheduleDictionary);

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
