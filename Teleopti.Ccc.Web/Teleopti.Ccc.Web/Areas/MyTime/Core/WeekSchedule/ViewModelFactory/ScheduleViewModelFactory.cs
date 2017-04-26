using System.Collections.Generic;
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
		private readonly WeekScheduleViewModelMapper _scheduleViewModelMapper;
		private readonly IWeekScheduleDomainDataProvider _weekScheduleDomainDataProvider;
		private readonly IMonthScheduleDomainDataProvider _monthScheduleDomainDataProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IStaffingPossibilityViewModelFactory _staffingPossibilityViewModelFactory;
		private readonly INow _now;

		public ScheduleViewModelFactory(MonthScheduleViewModelMapper monthMapper,
			WeekScheduleViewModelMapper scheduleViewModelMapper,
			IWeekScheduleDomainDataProvider weekScheduleDomainDataProvider,
			IMonthScheduleDomainDataProvider monthScheduleDomainDataProvider, ILoggedOnUser loggedOnUser,
			IStaffingPossibilityViewModelFactory staffingPossibilityViewModelFactory, INow now)
		{
			_monthMapper = monthMapper;
			_scheduleViewModelMapper = scheduleViewModelMapper;
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
			var weekDomainData = _weekScheduleDomainDataProvider.GetWeekSchedule(date);
			weekDomainData.SiteOpenHourIntradayPeriod = getIntradaySiteOpenHourPeriod();
			var minMaxTimeFixed = fixScheduleMinMaxTimeBySiteOpenHour(staffingPossiblityType, weekDomainData);
			if (minMaxTimeFixed && weekDomainData.Days != null)
			{
				foreach (var day in weekDomainData.Days)
				{
					day.MinMaxTime = weekDomainData.MinMaxTime;
				}
			}

			var weekScheduleViewModel = _scheduleViewModelMapper.Map(weekDomainData);
			weekScheduleViewModel.Possibilities = getPossibilities(date, staffingPossiblityType);
			return weekScheduleViewModel;
		}
		
		public DayScheduleViewModel CreateDayViewModel(DateOnly date, StaffingPossiblityType staffingPossiblityType)
		{
			var dayDomainData = _weekScheduleDomainDataProvider.GetDaySchedule(date);
			dayDomainData.SiteOpenHourIntradayPeriod = getIntradaySiteOpenHourPeriod();

			var minMaxTimeFixed = fixScheduleMinMaxTimeBySiteOpenHour(staffingPossiblityType, dayDomainData);
			if (minMaxTimeFixed)
			{
				dayDomainData.ScheduleDay.MinMaxTime = dayDomainData.MinMaxTime;
			}

			var dayScheduleViewModel = _scheduleViewModelMapper.Map(dayDomainData);
			dayScheduleViewModel.Possibilities = getPossibilities(date, staffingPossiblityType);

			return dayScheduleViewModel;
		}

		private bool fixScheduleMinMaxTimeBySiteOpenHour(StaffingPossiblityType staffingPossiblityType,
			BaseScheduleDomainData scheduleDomainData)
		{
			if (staffingPossiblityType != StaffingPossiblityType.Overtime)
			{
				return false;
			}

			var siteOpenHourPeriod = scheduleDomainData.SiteOpenHourIntradayPeriod;
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

		private IEnumerable<PeriodStaffingPossibilityViewModel> getPossibilities(DateOnly date,
			StaffingPossiblityType staffingPossiblityType)
		{
			return staffingPossiblityType != StaffingPossiblityType.None
				? _staffingPossibilityViewModelFactory.CreatePeriodStaffingPossibilityViewModels(date, staffingPossiblityType)
				: new PeriodStaffingPossibilityViewModel[] { };
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