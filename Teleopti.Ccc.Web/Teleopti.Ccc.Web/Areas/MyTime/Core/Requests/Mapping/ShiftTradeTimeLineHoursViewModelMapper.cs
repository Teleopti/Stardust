﻿using System;
using System.Collections.Generic;
using System.Linq;
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
			return _shiftTradeTimelineHoursViewModelFactory.CreateTimeLineHours(getTimeLinePeriod(mySchedule, possibleTradeSchedules, shiftTradeDate));
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
			if (mySchedule == null)
				return null;
			if (!mySchedule.ScheduleLayers.Any())
				return null;

			var timeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();

			return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(mySchedule.ScheduleLayers.First().Start,
			                                                            mySchedule.ScheduleLayers.Last().End, timeZone);

		}

		private DateTimePeriod? getpossibleTradeScheduleMinMax(
			IEnumerable<ShiftTradeAddPersonScheduleViewModel> possibleTradeSchedules)
		{
			var schedules = possibleTradeSchedules as IList<ShiftTradeAddPersonScheduleViewModel> ?? possibleTradeSchedules.ToList();

			if (!schedules.Any())
				return null;

			
			var timeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();

			var startTime = schedules.First().MinStart.GetValueOrDefault();
			var endTime = schedules.Max(l => l.ScheduleLayers.Last().End);

			return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startTime, endTime, timeZone);
		}
	}
}