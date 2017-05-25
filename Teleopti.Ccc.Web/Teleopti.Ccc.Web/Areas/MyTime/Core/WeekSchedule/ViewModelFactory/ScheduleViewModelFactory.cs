using System;
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
		private readonly IScheduleMinMaxTimeCalculator _scheduleMinMaxTimeCalculator;
		private readonly IStaffingDataAvailablePeriodProvider _staffingDataAvailablePeriodProvider;

		public ScheduleViewModelFactory(MonthScheduleViewModelMapper monthMapper,
			WeekScheduleViewModelMapper scheduleViewModelMapper,
			IWeekScheduleDomainDataProvider weekScheduleDomainDataProvider,
			IMonthScheduleDomainDataProvider monthScheduleDomainDataProvider,
			IPushMessageProvider pushMessageProvider,
			IScheduleMinMaxTimeCalculator scheduleMinMaxTimeCalculator, IStaffingDataAvailablePeriodProvider staffingDataAvailablePeriodProvider)
		{
			_monthMapper = monthMapper;
			_scheduleViewModelMapper = scheduleViewModelMapper;
			_weekScheduleDomainDataProvider = weekScheduleDomainDataProvider;
			_monthScheduleDomainDataProvider = monthScheduleDomainDataProvider;
			_pushMessageProvider = pushMessageProvider;
			_scheduleMinMaxTimeCalculator = scheduleMinMaxTimeCalculator;
			_staffingDataAvailablePeriodProvider = staffingDataAvailablePeriodProvider;
		}

		public MonthScheduleViewModel CreateMonthViewModel(DateOnly dateOnly)
		{
			var domainData = _monthScheduleDomainDataProvider.Get(dateOnly);
			return _monthMapper.Map(domainData);
		}

		public WeekScheduleViewModel CreateWeekViewModel(DateOnly date, StaffingPossiblityType staffingPossiblityType)
		{
			var weekDomainData = _weekScheduleDomainDataProvider.GetWeekSchedule(date);
			if (needAdjustTimeline(staffingPossiblityType, date, true))
			{
				_scheduleMinMaxTimeCalculator.AdjustScheduleMinMaxTime(weekDomainData);
			}

			var weekScheduleViewModel = _scheduleViewModelMapper.Map(weekDomainData, staffingPossiblityType == StaffingPossiblityType.Overtime);
			return weekScheduleViewModel;
		}

		public DayScheduleViewModel CreateDayViewModel(DateOnly date, StaffingPossiblityType staffingPossiblityType)
		{
			var daySchedule = _weekScheduleDomainDataProvider.GetDaySchedule(date);
			var hasVisualSchedule = hasAnyVisualSchedule(date, daySchedule);

			if (!hasVisualSchedule)
			{
				// Set timeline to 8:00-15:00 if no schedule
				// Refer to Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping.ShiftTradeTimeLineHoursViewModelMapper.getTimeLinePeriod()
				var defaultTimeLinePeriod = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(15));
				daySchedule.MinMaxTime = defaultTimeLinePeriod;
				daySchedule.ScheduleDay.MinMaxTime = defaultTimeLinePeriod;
			}

			if (needAdjustTimeline(staffingPossiblityType, date, false))
			{
				_scheduleMinMaxTimeCalculator.AdjustScheduleMinMaxTime(daySchedule);
			}

			daySchedule.UnReadMessageCount = _pushMessageProvider.UnreadMessageCount;

			var dayScheduleViewModel = _scheduleViewModelMapper.Map(daySchedule, staffingPossiblityType == StaffingPossiblityType.Overtime);
			return dayScheduleViewModel;
		}

		private bool periodIsVisible(DateTimePeriod? period, DateOnly date, TimeZoneInfo timeZone)
		{
			if (period == null) return false;

			var startDate = period.Value.StartDateTimeLocal(timeZone).Date;
			var endDate = period.Value.EndDateTimeLocal(timeZone).Date;
			return startDate == date.Date || (startDate < date.Date && endDate == date.Date);
		}

		private bool hasAnyVisualSchedule(DateOnly date, DayScheduleDomainData daySchedule)
		{
			var dayDomainData = daySchedule.ScheduleDay;
			var timeZone = dayDomainData.ScheduleDay?.TimeZone;

			var hasVisualLayerToday = dayDomainData.Projection.Any(p => periodIsVisible(p.Period, date, timeZone));
			var hasVisualLayerYesterday = dayDomainData.ProjectionYesterday.Any(p => periodIsVisible(p.Period, date, timeZone));

			var hasVisibleOvertimeToday = periodIsVisible(dayDomainData.OvertimeAvailability?.Period, date, timeZone);
			var hasVisibleOvertimeYesterday = periodIsVisible(dayDomainData.OvertimeAvailabilityYesterday?.Period, date, timeZone);

			var hasVisualSchedule = hasVisualLayerToday || hasVisualLayerYesterday || hasVisibleOvertimeToday || hasVisibleOvertimeYesterday;
			return hasVisualSchedule;
		}

		private bool needAdjustTimeline(StaffingPossiblityType staffingPossiblityType, DateOnly date, bool forThisWeek)
		{
			return staffingPossiblityType == StaffingPossiblityType.Overtime &&
				   _staffingDataAvailablePeriodProvider.GetPeriod(date, forThisWeek).HasValue;
		}
	}
}