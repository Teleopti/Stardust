﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    /// <summary>
    /// Helper for schedules
    /// </summary>
    public static class ScheduleHelper
    {
        /// <summary>
        /// Return a list with schedules that has freedays
        /// </summary>
        /// <param name="schedules"></param>
        /// <returns></returns>
        public static IList<IScheduleDay> SchedulesWithFreeDay(IList<IScheduleDay> schedules)
        {
            IList<IScheduleDay> schedulesWithFreeDays = new List<IScheduleDay>();

            foreach (IScheduleDay schedule in schedules)
            {
                if (schedule.PersonDayOffCollection().Count > 0)
                    schedulesWithFreeDays.Add(schedule);
            }

            return schedulesWithFreeDays;
        }

		/// <summary>
		/// Return a list with schedules that has specified shiftcategory
		/// </summary>
		/// <param name="schedules"></param>
		/// <param name="shiftCategory"></param>
		/// <returns></returns>
		public static IList<IScheduleDay> SchedulesWithShiftCategory(IList<IScheduleDay> schedules, IEntity shiftCategory)
		{
			IList<IScheduleDay> schedulesWithShiftCategory = new List<IScheduleDay>();

			foreach (IScheduleDay schedule in schedules)
			{
				if (schedule.SignificantPartForDisplay() == SchedulePartView.MainShift && schedule.AssignmentHighZOrder().ShiftCategory.Equals(shiftCategory))
					schedulesWithShiftCategory.Add(schedule);
			}

			return schedulesWithShiftCategory;
		}

		public static IList<IScheduleDay> SchedulesWithShiftCategory(IList<IScheduleDay> schedules)
		{
			IList<IScheduleDay> schedulesWithShiftCategory = new List<IScheduleDay>();

			foreach (IScheduleDay schedule in schedules)
			{
				if (schedule.SignificantPartForDisplay() == SchedulePartView.MainShift)
					schedulesWithShiftCategory.Add(schedule);
			}

			return schedulesWithShiftCategory;
		}

        /// <summary>
        /// Returns a list of schedules that contains days off 
        /// corresponding to the provided DayOffTemplate
        /// </summary>
        /// <param name="schedules">The schedules.</param>
        /// <param name="dayOffTemplate">The day off template.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ZoeT
        /// Created date: 2009-04-17
        /// </remarks>
        public static IList<IScheduleDay> SchedulesWithSpecificDayOff(IList<IScheduleDay> schedules, IDayOffTemplate dayOffTemplate)
        {
            IList<IScheduleDay> schedulesWithDayOff = new List<IScheduleDay>();

            foreach (IScheduleDay schedule in schedules)
            {
                foreach (IPersonDayOff dayOff in schedule.PersonDayOffCollection())
                {
                    if (dayOff.CompareToTemplateForLocking(dayOffTemplate))
                        schedulesWithDayOff.Add(schedule);
                }
            }
            return schedulesWithDayOff;
        }

        /// <summary>
        /// Return a list with schedules that has specified personAbsence
        /// </summary>
        /// <param name="schedules"></param>
        /// <param name="absence"></param>
        /// <returns></returns>
        public static IList<IScheduleDay> SchedulesWithAbsence(IList<IScheduleDay> schedules, IEntity absence)
        {
            IList<IScheduleDay> schedulesWithAbsence = new List<IScheduleDay>();

            foreach (IScheduleDay schedule in schedules)
            {
                IProjectionService projSvc = schedule.ProjectionService();
                IVisualLayerCollection res = projSvc.CreateProjection();

                if (res.Any())
                {
                    //get each visual layer for schedule
                    if (res.FilterLayers<IAbsence>().Any(layer => layer.Payload.Equals(absence)))
                    {
                    	schedulesWithAbsence.Add(schedule);
                    }
                }
                else
                {
                	IList<IPersonAbsence> abses = schedule.PersonAbsenceCollection();
                	if (abses.Any(abs => abs.Layer.Payload.Equals(absence)))
                	{
                		schedulesWithAbsence.Add(schedule);
                	}
                }
            }

            return schedulesWithAbsence;
        }
        public static IList<IScheduleDay> SchedulesWithAbsence(IList<IScheduleDay> schedules)
        {
            IList<IScheduleDay> schedulesWithAbsence = new List<IScheduleDay>();

            foreach (IScheduleDay schedule in schedules)
            {
                if (schedule.PersonAbsenceCollection().Count > 0)
                    schedulesWithAbsence.Add(schedule);
            }

            return schedulesWithAbsence;
        }

        public static TimeSpan ContractedTime(IProjectionSource projectionSource)
        {
            IProjectionService projSvc = projectionSource.ProjectionService();
            IVisualLayerCollection res = projSvc.CreateProjection();
            return res.ContractTime();
        }
    }

    public class ScheduleDisplayRow
    {
        public IScheduleDay ScheduleDayBefore { get; set; }
        public IScheduleDay ScheduleDay { get; set; }
        public IScheduleDay ScheduleDayAfter { get; set; }
        public DateTimePeriod MeetingPeriod { get; set; }
    }
}
