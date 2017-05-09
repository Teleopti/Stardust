﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class ScheduleViewModelFactory : IScheduleViewModelFactory
	{
		private readonly MonthScheduleViewModelMapper _monthMapper;
		private readonly WeekScheduleViewModelMapper _scheduleViewModelMapper;
		private readonly IWeekScheduleDomainDataProvider _weekScheduleDomainDataProvider;
		private readonly IMonthScheduleDomainDataProvider _monthScheduleDomainDataProvider;
		private readonly IPushMessageProvider _pushMessageProvider;
		private readonly IScheduleMinMaxTimeSiteOpenHourCalculator _scheduleMinMaxTimeSiteOpenHourCalculator;


		public ScheduleViewModelFactory(MonthScheduleViewModelMapper monthMapper,
			WeekScheduleViewModelMapper scheduleViewModelMapper,
			IWeekScheduleDomainDataProvider weekScheduleDomainDataProvider,
			IMonthScheduleDomainDataProvider monthScheduleDomainDataProvider,
			IPushMessageProvider pushMessageProvider,
			IScheduleMinMaxTimeSiteOpenHourCalculator scheduleMinMaxTimeSiteOpenHourCalculator)
		{
			_monthMapper = monthMapper;
			_scheduleViewModelMapper = scheduleViewModelMapper;
			_weekScheduleDomainDataProvider = weekScheduleDomainDataProvider;
			_monthScheduleDomainDataProvider = monthScheduleDomainDataProvider;
			_pushMessageProvider = pushMessageProvider;
			_scheduleMinMaxTimeSiteOpenHourCalculator = scheduleMinMaxTimeSiteOpenHourCalculator;
		}

		public MonthScheduleViewModel CreateMonthViewModel(DateOnly dateOnly)
		{
			var domainData = _monthScheduleDomainDataProvider.Get(dateOnly);
			return _monthMapper.Map(domainData);
		}

		public WeekScheduleViewModel CreateWeekViewModel(DateOnly date, StaffingPossiblityType staffingPossiblityType)
		{
			var weekDomainData = _weekScheduleDomainDataProvider.GetWeekSchedule(date);
			if (staffingPossiblityType == StaffingPossiblityType.Overtime)
			{
				_scheduleMinMaxTimeSiteOpenHourCalculator.AdjustScheduleMinMaxTime(weekDomainData);
			}

			var weekScheduleViewModel = _scheduleViewModelMapper.Map(weekDomainData);
			return weekScheduleViewModel;
		}

		public DayScheduleViewModel CreateDayViewModel(DateOnly date, StaffingPossiblityType staffingPossiblityType)
		{
			var dayDomainData = _weekScheduleDomainDataProvider.GetDaySchedule(date);
			var scheduleDay = dayDomainData.ScheduleDay;

			var hasAnySchedule = scheduleDay.Projection.Any() || scheduleDay.ProjectionYesterday.Any();
			var hasAnyOvertime = scheduleDay.OvertimeAvailability != null || scheduleDay.OvertimeAvailabilityYesterday != null;

			if (hasAnySchedule || hasAnyOvertime)
			{
				if (staffingPossiblityType == StaffingPossiblityType.Overtime)
				{
					_scheduleMinMaxTimeSiteOpenHourCalculator.AdjustScheduleMinMaxTime(dayDomainData);
				}
			}
			else
			{
				// Set timeline to 8:00-15:00 if no schedule
				// Refer to Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping.ShiftTradeTimeLineHoursViewModelMapper.getTimeLinePeriod()
				var defaultTimeLinePeriod = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(15));
				dayDomainData.MinMaxTime = defaultTimeLinePeriod;
				scheduleDay.MinMaxTime = defaultTimeLinePeriod;
			}

			dayDomainData.UnReadMessageCount = _pushMessageProvider.UnreadMessageCount;

			var dayScheduleViewModel = _scheduleViewModelMapper.Map(dayDomainData);
			return dayScheduleViewModel;
		}
	}
}