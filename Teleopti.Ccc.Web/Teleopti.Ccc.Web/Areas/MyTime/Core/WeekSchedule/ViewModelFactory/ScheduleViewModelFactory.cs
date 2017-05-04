using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly INow _now;
		private readonly IPushMessageProvider _pushMessageProvider;

		public ScheduleViewModelFactory(MonthScheduleViewModelMapper monthMapper,
			WeekScheduleViewModelMapper scheduleViewModelMapper,
			IWeekScheduleDomainDataProvider weekScheduleDomainDataProvider,
			IMonthScheduleDomainDataProvider monthScheduleDomainDataProvider, 
			ILoggedOnUser loggedOnUser, 
			INow now,
			IPushMessageProvider pushMessageProvider)
		{
			_monthMapper = monthMapper;
			_scheduleViewModelMapper = scheduleViewModelMapper;
			_weekScheduleDomainDataProvider = weekScheduleDomainDataProvider;
			_monthScheduleDomainDataProvider = monthScheduleDomainDataProvider;
			_loggedOnUser = loggedOnUser;
			_now = now;
			_pushMessageProvider = pushMessageProvider;

		}

		public MonthScheduleViewModel CreateMonthViewModel(DateOnly dateOnly)
		{
			var domainData = _monthScheduleDomainDataProvider.Get(dateOnly);
			return _monthMapper.Map(domainData);
		}

		public WeekScheduleViewModel CreateWeekViewModel(DateOnly date, StaffingPossiblityType staffingPossiblityType)
		{
			var weekDomainData = _weekScheduleDomainDataProvider.GetWeekSchedule(date);
			var minMaxTimeFixed = fixScheduleMinMaxTimeBySiteOpenHour(staffingPossiblityType, weekDomainData);
			if (minMaxTimeFixed && weekDomainData.Days != null)
			{
				foreach (var day in weekDomainData.Days)
				{
					day.MinMaxTime = weekDomainData.MinMaxTime;
				}
			}

			var weekScheduleViewModel = _scheduleViewModelMapper.Map(weekDomainData);
			return weekScheduleViewModel;
		}
		
		public DayScheduleViewModel CreateDayViewModel(DateOnly date, StaffingPossiblityType staffingPossiblityType)
		{
			var dayDomainData = _weekScheduleDomainDataProvider.GetDaySchedule(date);
			var personAssignment = dayDomainData.ScheduleDay.ScheduleDay.PersonAssignment();

			if (!dayDomainData.ScheduleDay.Projection.Any() && personAssignment != null &&
				!personAssignment.ShiftLayers.OfType<OvertimeShiftLayer>().Any())
			{
				// Set timeline to 8:00-15:00 if no schedule
				// Refer to Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping.ShiftTradeTimeLineHoursViewModelMapper.getTimeLinePeriod()
				var defaultTimeLinePeriod = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(15));
				dayDomainData.MinMaxTime = defaultTimeLinePeriod;
				dayDomainData.ScheduleDay.MinMaxTime = defaultTimeLinePeriod;
			}
			else
			{
				var minMaxTimeFixed = fixScheduleMinMaxTimeBySiteOpenHour(staffingPossiblityType, dayDomainData);
				if (minMaxTimeFixed)
				{
					dayDomainData.ScheduleDay.MinMaxTime = dayDomainData.MinMaxTime;
				}
			}
			dayDomainData.UnReadMessageCount = _pushMessageProvider.UnreadMessageCount;

			var dayScheduleViewModel = _scheduleViewModelMapper.Map(dayDomainData);

			return dayScheduleViewModel;
		}

		private bool fixScheduleMinMaxTimeBySiteOpenHour(StaffingPossiblityType staffingPossiblityType,
			BaseScheduleDomainData scheduleDomainData)
		{
			if (staffingPossiblityType != StaffingPossiblityType.Overtime)
			{
				return false;
			}

			var siteOpenHourPeriod = getIntradaySiteOpenHourPeriod();
			if (!siteOpenHourPeriod.HasValue)
			{
				return false;
			}

			var  newTimelinePeriod = getTimelinePeriod(scheduleDomainData, (TimePeriod)siteOpenHourPeriod);
			if (scheduleDomainData.MinMaxTime == newTimelinePeriod)
			{
				return false;
			}

			scheduleDomainData.MinMaxTime = newTimelinePeriod;
			return true;
		}

		private static TimePeriod getTimelinePeriod(BaseScheduleDomainData scheduleDomainData, TimePeriod siteOpenHourPeriod)
		{
			var scheduleMinMaxTime = scheduleDomainData.MinMaxTime;
			var minTime = scheduleMinMaxTime.StartTime;
			var maxTime = scheduleMinMaxTime.EndTime;
			if (siteOpenHourPeriod.StartTime < minTime)
			{
				minTime = siteOpenHourPeriod.StartTime;
			}
			if (siteOpenHourPeriod.EndTime > maxTime)
			{
				maxTime = siteOpenHourPeriod.EndTime;
			}

			return minTime == scheduleMinMaxTime.StartTime && maxTime == scheduleMinMaxTime.EndTime
				? scheduleMinMaxTime
				: new TimePeriod(minTime, maxTime);
		}

		private TimePeriod? getIntradaySiteOpenHourPeriod()
		{
			var siteOpenHour = _loggedOnUser.CurrentUser().SiteOpenHour(_now.LocalDateOnly());
			if (siteOpenHour == null || siteOpenHour.IsClosed)
			{
				return null;
			}
			return siteOpenHour.TimePeriod;
		}
	}
}