﻿using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class ScheduleViewModelFactory : IScheduleViewModelFactory
	{
		private readonly MonthScheduleViewModelMapper _monthMapper;
		private readonly WeekScheduleViewModelMapper _scheduleViewModelMapper;
		private readonly IWeekScheduleDomainDataProvider _weekScheduleDomainDataProvider;
		private readonly IMonthScheduleDomainDataProvider _monthScheduleDomainDataProvider;
		private readonly IScheduleWeekMinMaxTimeCalculator _scheduleWeekMinMaxTimeCalculator;
		private readonly IStaffingDataAvailablePeriodProvider _staffingDataAvailablePeriodProvider;

		public ScheduleViewModelFactory(MonthScheduleViewModelMapper monthMapper,
			WeekScheduleViewModelMapper scheduleViewModelMapper,
			IWeekScheduleDomainDataProvider weekScheduleDomainDataProvider,
			IMonthScheduleDomainDataProvider monthScheduleDomainDataProvider,
			IScheduleWeekMinMaxTimeCalculator scheduleWeekMinMaxTimeCalculator,
			IStaffingDataAvailablePeriodProvider staffingDataAvailablePeriodProvider)
		{
			_monthMapper = monthMapper;
			_scheduleViewModelMapper = scheduleViewModelMapper;
			_weekScheduleDomainDataProvider = weekScheduleDomainDataProvider;
			_monthScheduleDomainDataProvider = monthScheduleDomainDataProvider;
			_scheduleWeekMinMaxTimeCalculator = scheduleWeekMinMaxTimeCalculator;
			_staffingDataAvailablePeriodProvider = staffingDataAvailablePeriodProvider;
		}

		public MonthScheduleViewModel CreateMonthViewModel(DateOnly dateOnly)
		{
			var domainData = _monthScheduleDomainDataProvider.Get(dateOnly, true);
			return _monthMapper.Map(domainData);
		}

		public MonthScheduleViewModel CreateMobileMonthViewModel(DateOnly dateOnly)
		{
			var domainData = _monthScheduleDomainDataProvider.Get(dateOnly, false);
			return _monthMapper.Map(domainData, true);
		}

		public WeekScheduleViewModel CreateWeekViewModel(DateOnly date, StaffingPossiblityType staffingPossiblityType)
		{
			var weekDomainData = _weekScheduleDomainDataProvider.GetWeekSchedule(date);
			if (needAdjustTimeline(staffingPossiblityType, date, true))
			{
				_scheduleWeekMinMaxTimeCalculator.AdjustScheduleMinMaxTime(weekDomainData);
			}

			var isOvertimeStaffingPossiblity = staffingPossiblityType == StaffingPossiblityType.Overtime;
			var weekScheduleViewModel = _scheduleViewModelMapper.Map(weekDomainData, isOvertimeStaffingPossiblity);
			return weekScheduleViewModel;
		}

		private bool needAdjustTimeline(StaffingPossiblityType staffingPossiblityType, DateOnly date, bool forThisWeek)
		{
			return staffingPossiblityType == StaffingPossiblityType.Overtime &&
				   _staffingDataAvailablePeriodProvider.GetPeriodForAbsence(date, forThisWeek).HasValue;
		}
	}
}