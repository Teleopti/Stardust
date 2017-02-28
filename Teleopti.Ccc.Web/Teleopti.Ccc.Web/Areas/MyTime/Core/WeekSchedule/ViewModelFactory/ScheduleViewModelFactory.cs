﻿using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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

		public ScheduleViewModelFactory(MonthScheduleViewModelMapper monthMapper, WeekScheduleViewModelMapper weekMapper, IWeekScheduleDomainDataProvider weekScheduleDomainDataProvider,
			IMonthScheduleDomainDataProvider monthScheduleDomainDataProvider, ILoggedOnUser loggedOnUser,
			IStaffingPossibilityViewModelFactory staffingPossibilityViewModelFactory)
		{
			_monthMapper = monthMapper;
			_weekMapper = weekMapper;
			_weekScheduleDomainDataProvider = weekScheduleDomainDataProvider;
			_monthScheduleDomainDataProvider = monthScheduleDomainDataProvider;
			_loggedOnUser = loggedOnUser;
			_staffingPossibilityViewModelFactory = staffingPossibilityViewModelFactory;
		}

		public MonthScheduleViewModel CreateMonthViewModel(DateOnly dateOnly)
		{
			var domainData = _monthScheduleDomainDataProvider.Get(dateOnly);
			return _monthMapper.Map(domainData);
		}

		public WeekScheduleViewModel CreateWeekViewModel(DateOnly dateOnly, StaffingPossiblityType staffingPossiblityType)
		{
			var domainData = _weekScheduleDomainDataProvider.Get(dateOnly);
			domainData.SiteOpenHourIntradayPeriod = getIntradaySiteOpenHourPeriod();
			adjustScheduleMinMaxTimeBySiteOpenHour(staffingPossiblityType, domainData);
			var weekScheduleViewModel = _weekMapper.Map(domainData);
			setPossibilities(staffingPossiblityType, weekScheduleViewModel);
			return weekScheduleViewModel;
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

		private void setPossibilities(StaffingPossiblityType staffingPossiblityType, WeekScheduleViewModel weekScheduleViewModel)
		{
			if (staffingPossiblityType != StaffingPossiblityType.None && containsIntradaySchedule(weekScheduleViewModel))
			{
				weekScheduleViewModel.Possibilities =
					_staffingPossibilityViewModelFactory.CreateIntradayPeriodStaffingPossibilityViewModels(staffingPossiblityType);
			}
			else
			{
				weekScheduleViewModel.Possibilities = new PeriodStaffingPossibilityViewModel[] { };
			}
		}

		private static bool containsIntradaySchedule(WeekScheduleViewModel weekScheduleViewModel)
		{
			return new DateOnlyPeriod(new DateOnly(weekScheduleViewModel.PeriodSelection.StartDate),
				new DateOnly(weekScheduleViewModel.PeriodSelection.EndDate)).Contains(DateOnly.Today);
		}

		private TimePeriod? getIntradaySiteOpenHourPeriod()
		{
			var siteOpenHour = _loggedOnUser.CurrentUser().SiteOpenHour(DateOnly.Today);
			if (siteOpenHour == null || siteOpenHour.IsClosed)
			{
				return null;
			}
			return siteOpenHour.TimePeriod;
		}
	}
}