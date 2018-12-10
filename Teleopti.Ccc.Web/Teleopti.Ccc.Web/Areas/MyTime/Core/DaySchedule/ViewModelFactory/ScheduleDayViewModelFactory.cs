using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Staffing;
using Teleopti.Ccc.Web.Areas.MyTime.Core.DaySchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.DaySchedule;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.DaySchedule.ViewModelFactory
{
	public class ScheduleDayViewModelFactory : IScheduleDayViewModelFactory
	{
		private readonly DayScheduleViewModelMapper _scheduleViewModelMapper;
		private readonly IDayScheduleDomainDataProvider _dayScheduleDomainDataProvider;
		private readonly IPushMessageProvider _pushMessageProvider;
		private readonly IScheduleDayMinMaxTimeCalculator _scheduleDayMinMaxTimeCalculator;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IStaffingDataAvailablePeriodProvider _staffingDataAvailablePeriodProvider;

		public ScheduleDayViewModelFactory(
			DayScheduleViewModelMapper scheduleViewModelMapper,
			IPushMessageProvider pushMessageProvider,
			IStaffingDataAvailablePeriodProvider staffingDataAvailablePeriodProvider,
			IDayScheduleDomainDataProvider dayScheduleDomainDataProvider,
			IScheduleDayMinMaxTimeCalculator scheduleDayMinMaxTimeCalculator,
			ILoggedOnUser loggedOnUser)
		{
			_scheduleViewModelMapper = scheduleViewModelMapper;
			_pushMessageProvider = pushMessageProvider;
			_staffingDataAvailablePeriodProvider = staffingDataAvailablePeriodProvider;
			_dayScheduleDomainDataProvider = dayScheduleDomainDataProvider;
			_scheduleDayMinMaxTimeCalculator = scheduleDayMinMaxTimeCalculator;
			_loggedOnUser = loggedOnUser;
		}

		public DayScheduleViewModel CreateDayViewModel(DateOnly date, StaffingPossiblityType staffingPossiblityType)
		{
			var daySchedule = _dayScheduleDomainDataProvider.GetDaySchedule(date);
			var hasVisualSchedule = hasAnyVisualSchedule(date, daySchedule);

			if (!hasVisualSchedule)
			{
				var defaultTimeLinePeriod = new TimePeriod(TimeSpan.FromHours(DefaultSchedulePeriodProvider.DefaultStartHour), TimeSpan.FromHours(DefaultSchedulePeriodProvider.DefaultEndHour));
				daySchedule.MinMaxTime = defaultTimeLinePeriod;
				daySchedule.MinMaxTime = defaultTimeLinePeriod;
			}

			if (needAdjustTimeline(staffingPossiblityType, date, false))
			{
				_scheduleDayMinMaxTimeCalculator.AdjustScheduleMinMaxTime(daySchedule);
			}

			daySchedule.UnReadMessageCount = _pushMessageProvider.UnreadMessageCount;

			var isOvertimeStaffingPossiblity = staffingPossiblityType == StaffingPossiblityType.Overtime;
			var dayScheduleViewModel = _scheduleViewModelMapper.Map(daySchedule, isOvertimeStaffingPossiblity);
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
			if (daySchedule?.Projection == null)
				return false;

			var timeZone = daySchedule?.ScheduleDay.TimeZone;
			var hasVisualLayerToday = daySchedule.Projection.Any(p => periodIsVisible(p.Period, date, timeZone));
			var hasVisualLayerYesterday = daySchedule.ProjectionYesterday != null && daySchedule.ProjectionYesterday.Any(p => periodIsVisible(p.Period, date, timeZone));

			var hasVisibleOvertimeToday = periodIsVisible(daySchedule.OvertimeAvailability?.Period, date, timeZone);
			var hasVisibleOvertimeYesterday = existsVisibleOvertimeYesterday(date, daySchedule, timeZone);

			var hasVisualSchedule = hasVisualLayerToday || hasVisualLayerYesterday ||
									hasVisibleOvertimeToday || hasVisibleOvertimeYesterday;
			return hasVisualSchedule;
		}

		private bool existsVisibleOvertimeYesterday(DateOnly date, DayScheduleDomainData dayDomainData,
			TimeZoneInfo timeZone)
		{
			// Use StartTime to EndTime to rebuild actual period of OvertimeAvailability and check if it's visible,
			// Since OvertimeAvailability.Period does NOT present actual period from StartTime to EndTime
			var yesterdayOvertimeAvailability = dayDomainData.OvertimeAvailabilityYesterday;
			if (yesterdayOvertimeAvailability?.StartTime == null || yesterdayOvertimeAvailability.EndTime == null)
			{
				return false;
			}

			var previousDate = date.Date.AddDays(-1);
			var startTime = previousDate.Add(yesterdayOvertimeAvailability.StartTime.Value);
			var endTime = previousDate.Add(yesterdayOvertimeAvailability.EndTime.Value);
			if (endTime == date.Date)
			{
				endTime = endTime.AddMinutes(-1);
			}

			var utcStartTime = TimeZoneHelper.ConvertToUtc(startTime, timeZone);
			var utcEndTime = TimeZoneHelper.ConvertToUtc(endTime, timeZone);
			var period = new DateTimePeriod(utcStartTime, utcEndTime);

			return periodIsVisible(period, date, timeZone);
		}

		private bool needAdjustTimeline(StaffingPossiblityType staffingPossiblityType, DateOnly date, bool forThisWeek)
		{
			return staffingPossiblityType == StaffingPossiblityType.Overtime &&
				   _staffingDataAvailablePeriodProvider.GetPeriodForAbsence(_loggedOnUser.CurrentUser(), date, forThisWeek).HasValue;
		}
	}
}