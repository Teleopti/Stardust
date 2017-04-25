using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule;
using Teleopti.Ccc.Web.Areas.MyTime.Models.ScheduleStaffingPossibility;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class ScheduleViewModelFactory : IScheduleViewModelFactory
	{
		private readonly MonthScheduleViewModelMapper _monthMapper;
		private readonly WeekScheduleViewModelMapper _weekMapper;
		private readonly IWeekScheduleDomainDataProvider _weekScheduleDomainDataProvider;
		private readonly IMonthScheduleDomainDataProvider _monthScheduleDomainDataProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IStaffingPossibilityViewModelFactory _staffingPossibilityViewModelFactory;
		private readonly INow _now;

		public ScheduleViewModelFactory(MonthScheduleViewModelMapper monthMapper, WeekScheduleViewModelMapper weekMapper, IWeekScheduleDomainDataProvider weekScheduleDomainDataProvider,
			IMonthScheduleDomainDataProvider monthScheduleDomainDataProvider, ILoggedOnUser loggedOnUser,
			IStaffingPossibilityViewModelFactory staffingPossibilityViewModelFactory, INow now)
		{
			_monthMapper = monthMapper;
			_weekMapper = weekMapper;
			_weekScheduleDomainDataProvider = weekScheduleDomainDataProvider;
			_monthScheduleDomainDataProvider = monthScheduleDomainDataProvider;
			_loggedOnUser = loggedOnUser;
			_staffingPossibilityViewModelFactory = staffingPossibilityViewModelFactory;
			_now = now;
		}

		public MonthScheduleViewModel CreateMonthViewModel(DateOnly dateOnly)
		{
			var domainData = _monthScheduleDomainDataProvider.Get(dateOnly);
			return _monthMapper.Map(domainData);
		}

		public WeekScheduleViewModel CreateWeekViewModel(DateOnly date, StaffingPossiblityType staffingPossiblityType)
		{
			var domainData = _weekScheduleDomainDataProvider.Get(date);
			domainData.SiteOpenHourIntradayPeriod = getIntradaySiteOpenHourPeriod();
			adjustScheduleMinMaxTimeBySiteOpenHour(staffingPossiblityType, domainData);
			var weekScheduleViewModel = _weekMapper.Map(domainData);
			setPossibilities(date, staffingPossiblityType, weekScheduleViewModel);
			return weekScheduleViewModel;
		}

		// TODO-xinfli: Temporary implement for front-end debug only
		public DayScheduleViewModel CreateDayViewModel(DateOnly date, StaffingPossiblityType staffingPossiblityType)
		{
			var domainData = _weekScheduleDomainDataProvider.Get(date);
			domainData.SiteOpenHourIntradayPeriod = getIntradaySiteOpenHourPeriod();
			adjustScheduleMinMaxTimeBySiteOpenHour(staffingPossiblityType, domainData);
			var weekScheduleViewModel = _weekMapper.Map(domainData);
			setPossibilities(date, staffingPossiblityType, weekScheduleViewModel);

			var scheduleForThisDate = weekScheduleViewModel.Days.SingleOrDefault(d => d.Date == date.Date.ToShortDateString());
			return new DayScheduleViewModel
			{
				Date = scheduleForThisDate == null ? date.ToShortDateString() : scheduleForThisDate.Date,
				Schedule = scheduleForThisDate,
				RequestPermission = weekScheduleViewModel.RequestPermission,
				TimeLineCulture = weekScheduleViewModel.TimeLineCulture,
				TimeLine = weekScheduleViewModel.TimeLine,
				AsmPermission = weekScheduleViewModel.AsmPermission,
				ViewPossibilityPermission = weekScheduleViewModel.ViewPossibilityPermission,
				IsToday = date.Date == DateTime.Now.Date,
				DatePickerFormat = weekScheduleViewModel.DatePickerFormat,
				DaylightSavingTimeAdjustment = weekScheduleViewModel.DaylightSavingTimeAdjustment,
				BaseUtcOffsetInMinutes = weekScheduleViewModel.BaseUtcOffsetInMinutes,
				CheckStaffingByIntraday = weekScheduleViewModel.CheckStaffingByIntraday,
				Possibilities = weekScheduleViewModel.Possibilities,
				SiteOpenHourIntradayPeriod = weekScheduleViewModel.SiteOpenHourIntradayPeriod
			};
		}

		private void adjustScheduleMinMaxTimeBySiteOpenHour(StaffingPossiblityType staffingPossiblityType,
			WeekScheduleDomainData weekScheduleDomainData)
		{
			if (staffingPossiblityType != StaffingPossiblityType.Overtime)
				return;

			var scheduleMinMaxTime = weekScheduleDomainData.MinMaxTime;
			var siteOpenHourPeriod = weekScheduleDomainData.SiteOpenHourIntradayPeriod;
			if (!siteOpenHourPeriod.HasValue)
				return;

			var minTime = scheduleMinMaxTime.StartTime;
			var maxTime = scheduleMinMaxTime.EndTime;
			if (siteOpenHourPeriod.Value.StartTime < minTime)
			{
				minTime = siteOpenHourPeriod.Value.StartTime;
			}
			if (siteOpenHourPeriod.Value.EndTime > maxTime)
			{
				maxTime = siteOpenHourPeriod.Value.EndTime;
			}

			if (minTime == scheduleMinMaxTime.StartTime && maxTime == scheduleMinMaxTime.EndTime)
				return;

			var newTimelinePeriod = new TimePeriod(minTime, maxTime);
			weekScheduleDomainData.MinMaxTime = newTimelinePeriod;

			if (weekScheduleDomainData.Days == null) return;

			foreach (var day in weekScheduleDomainData.Days)
			{
				day.MinMaxTime = newTimelinePeriod;
			}
		}

		private void setPossibilities(DateOnly date, StaffingPossiblityType staffingPossiblityType
			, WeekScheduleViewModel weekScheduleViewModel)
		{
			weekScheduleViewModel.Possibilities = staffingPossiblityType != StaffingPossiblityType.None
				? _staffingPossibilityViewModelFactory.CreatePeriodStaffingPossibilityViewModels(date, staffingPossiblityType)
				: new PeriodStaffingPossibilityViewModel[] {};
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