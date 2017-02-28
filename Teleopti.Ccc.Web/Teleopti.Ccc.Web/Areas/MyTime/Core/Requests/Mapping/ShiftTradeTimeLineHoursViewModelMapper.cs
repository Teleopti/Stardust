﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeTimeLineHoursViewModelMapper : IShiftTradeTimeLineHoursViewModelMapper
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IShiftTradeTimeLineHoursViewModelFactory _shiftTradeTimelineHoursViewModelFactory;

		public ShiftTradeTimeLineHoursViewModelMapper(ILoggedOnUser loggedOnUser, IShiftTradeTimeLineHoursViewModelFactory shiftTradeTimelineHoursViewModelFactory)
		{
			_loggedOnUser = loggedOnUser;
			_shiftTradeTimelineHoursViewModelFactory = shiftTradeTimelineHoursViewModelFactory;
		}

		public IEnumerable<ShiftTradeTimeLineHoursViewModel> Map(ShiftTradeAddPersonScheduleViewModel mySchedule,
																 IEnumerable<ShiftTradeAddPersonScheduleViewModel>
																	 possibleTradeSchedules, DateOnly shiftTradeDate)
		{
			var timeLinePeriod = getTimeLinePeriod(mySchedule, possibleTradeSchedules, shiftTradeDate);
			var timeLine = _shiftTradeTimelineHoursViewModelFactory.CreateTimeLineHours(timeLinePeriod);
			return timeLine;
		}

		private DateTimePeriod getTimeLinePeriod(ShiftTradeAddPersonScheduleViewModel mySchedule,
												 IEnumerable<ShiftTradeAddPersonScheduleViewModel> possibleTradeSchedules,
												 DateOnly shiftTradeDate)
		{
			DateTimePeriod? myScheduleMinMax = getMyScheduleMinMax(mySchedule);
			DateTimePeriod? possibleTradeScheduleMinMax = getpossibleTradeScheduleMinMax(possibleTradeSchedules);

			var timeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();

			var returnPeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(shiftTradeDate.Date.AddHours(8),
																					shiftTradeDate.Date.AddHours(17), timeZone);

			if (myScheduleMinMax.HasValue)
				returnPeriod = possibleTradeScheduleMinMax.HasValue
								   ? myScheduleMinMax.Value.MaximumPeriod(possibleTradeScheduleMinMax.Value)
								   : myScheduleMinMax.Value;
			else if (possibleTradeScheduleMinMax.HasValue)
				returnPeriod = possibleTradeScheduleMinMax.Value;

			returnPeriod = returnPeriod.ChangeStartTime(new TimeSpan(0, -15, 0));
			returnPeriod = returnPeriod.ChangeEndTime(new TimeSpan(0, 15, 0));
			return returnPeriod;
		}

		private DateTimePeriod? getMyScheduleMinMax(ShiftTradeAddPersonScheduleViewModel mySchedule)
		{
			if (mySchedule == null || mySchedule.ScheduleLayers == null)
				return null;
			if (!mySchedule.ScheduleLayers.Any())
				return null;

			var start = mySchedule.ScheduleLayers.First().Start;
			var end = mySchedule.ScheduleLayers.Last().End;

			var timeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();

			var result = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(start, end, timeZone);
			return result;
		}

		private DateTimePeriod? getpossibleTradeScheduleMinMax(
			IEnumerable<ShiftTradeAddPersonScheduleViewModel> possibleTradeSchedules)
		{
			var schedules = (possibleTradeSchedules as IList<ShiftTradeAddPersonScheduleViewModel>) ?? possibleTradeSchedules.ToList();

			var schedulesWithoutDayoffAndEmptyDays = schedules.Where(s => s.IsDayOff == false && !s.ScheduleLayers.IsNullOrEmpty());
			var schedulesWithoutDOAndEmpty = schedulesWithoutDayoffAndEmptyDays as IList<ShiftTradeAddPersonScheduleViewModel> ??
									 schedulesWithoutDayoffAndEmptyDays.ToList();

			if (!schedulesWithoutDOAndEmpty.Any())
				return null;


			var timeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();

			var startTime = schedulesWithoutDOAndEmpty.Min(s => s.ScheduleLayers.First().Start);
			var endTime = schedulesWithoutDOAndEmpty.Max(l => l.ScheduleLayers.Last().End);

			return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startTime, endTime, timeZone);
		}
	}
}