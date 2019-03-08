using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Staffing;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.WeekSchedule;


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
		private readonly ILoggedOnUser _loggedOnUser;

		public ScheduleViewModelFactory(MonthScheduleViewModelMapper monthMapper,
			WeekScheduleViewModelMapper scheduleViewModelMapper,
			IWeekScheduleDomainDataProvider weekScheduleDomainDataProvider,
			IMonthScheduleDomainDataProvider monthScheduleDomainDataProvider,
			IScheduleWeekMinMaxTimeCalculator scheduleWeekMinMaxTimeCalculator,
			IStaffingDataAvailablePeriodProvider staffingDataAvailablePeriodProvider,
			ILoggedOnUser loggedOnUser)
		{
			_monthMapper = monthMapper;
			_scheduleViewModelMapper = scheduleViewModelMapper;
			_weekScheduleDomainDataProvider = weekScheduleDomainDataProvider;
			_monthScheduleDomainDataProvider = monthScheduleDomainDataProvider;
			_scheduleWeekMinMaxTimeCalculator = scheduleWeekMinMaxTimeCalculator;
			_staffingDataAvailablePeriodProvider = staffingDataAvailablePeriodProvider;
			_loggedOnUser = loggedOnUser;
		}

		public MonthScheduleViewModel CreateMonthViewModel(DateOnly dateOnly)
		{
			var domainData = _monthScheduleDomainDataProvider.GetMonthData(dateOnly);
			return _monthMapper.Map(domainData);
		}

		public MonthScheduleViewModel CreateMobileMonthViewModel(DateOnly dateOnly)
		{
			var domainData = _monthScheduleDomainDataProvider.GetMobileMonthData(dateOnly);
			return _monthMapper.Map(domainData, true);
		}

		public WeekScheduleViewModel CreateWeekViewModel(DateOnly date, StaffingPossibilityType staffingPossibilityType)
		{
			var weekDomainData = _weekScheduleDomainDataProvider.GetWeekSchedule(date);
			if (needAdjustTimeline(staffingPossibilityType, date, true))
			{
				_scheduleWeekMinMaxTimeCalculator.AdjustScheduleMinMaxTime(weekDomainData);
			}

			var isOvertimeStaffingPossiblity = staffingPossibilityType == StaffingPossibilityType.Overtime;
			var weekScheduleViewModel = _scheduleViewModelMapper.Map(weekDomainData, isOvertimeStaffingPossiblity);
			return weekScheduleViewModel;
		}

		private bool needAdjustTimeline(StaffingPossibilityType staffingPossibilityType, DateOnly date, bool forThisWeek)
		{
			return staffingPossibilityType == StaffingPossibilityType.Overtime &&
				   _staffingDataAvailablePeriodProvider.GetPeriodForAbsence(_loggedOnUser.CurrentUser(), date, forThisWeek).HasValue;
		}
	}
}