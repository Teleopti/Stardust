using System;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class ScheduleViewModelFactory : IScheduleViewModelFactory
	{
	    private readonly IMappingEngine _mapper;
		private readonly IWeekScheduleDomainDataProvider _weekScheduleDomainDataProvider;
		private readonly IMonthScheduleDomainDataProvider _monthScheduleDomainDataProvider;
		private readonly ILoggedOnUser _loggedOnUser;

		public ScheduleViewModelFactory(IMappingEngine mapper, IWeekScheduleDomainDataProvider weekScheduleDomainDataProvider, IMonthScheduleDomainDataProvider monthScheduleDomainDataProvider, ILoggedOnUser loggedOnUser)
		{
			_mapper = mapper;
			_weekScheduleDomainDataProvider = weekScheduleDomainDataProvider;
			_monthScheduleDomainDataProvider = monthScheduleDomainDataProvider;
			_loggedOnUser = loggedOnUser;
		}

		public MonthScheduleViewModel CreateMonthViewModel(DateOnly dateOnly)
	    {
			var domainData = _monthScheduleDomainDataProvider.Get(dateOnly);
			return _mapper.Map<MonthScheduleDomainData, MonthScheduleViewModel>(domainData);
        }

	    public WeekScheduleViewModel CreateWeekViewModel(DateOnly dateOnly, StaffingPossiblity staffingPossiblity)
	    {
			var domainData = _weekScheduleDomainDataProvider.Get(dateOnly);
			if (staffingPossiblity == StaffingPossiblity.Overtime)
			{
				domainData.MinMaxTime = adjustScheduleMinMaxTimeBySiteOpenHour(domainData.MinMaxTime);
			}
			return _mapper.Map<WeekScheduleDomainData, WeekScheduleViewModel>(domainData);
		}

		private TimePeriod adjustScheduleMinMaxTimeBySiteOpenHour(TimePeriod scheduleMinMaxTime)
		{
			var siteOpenHourPeriod = getIntradaySiteOpenHourPeriod();
			if (!siteOpenHourPeriod.HasValue)
			{
				return scheduleMinMaxTime;
			}
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
			return new TimePeriod(minTime, maxTime);
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