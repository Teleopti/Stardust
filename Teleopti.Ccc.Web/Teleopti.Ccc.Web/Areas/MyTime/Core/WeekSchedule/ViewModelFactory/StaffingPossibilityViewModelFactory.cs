using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Web.Areas.MyTime.Models.ScheduleStaffingPossibility;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class StaffingPossibilityViewModelFactory : IStaffingPossibilityViewModelFactory
	{
		private readonly IScheduleStaffingPossibilityCalculator _scheduleStaffingPossibilityCalculator;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly ILoggedOnUser _loggedOnUser;

		public StaffingPossibilityViewModelFactory(
			IScheduleStaffingPossibilityCalculator scheduleStaffingPossibilityCalculator, IIntervalLengthFetcher intervalLengthFetcher, ILoggedOnUser loggedOnUser)
		{
			_scheduleStaffingPossibilityCalculator = scheduleStaffingPossibilityCalculator;
			_intervalLengthFetcher = intervalLengthFetcher;
			_loggedOnUser = loggedOnUser;
		}

		public IEnumerable<PeriodStaffingPossibilityViewModel> CreatePeriodStaffingPossibilityViewModels(DateOnly startDate,
			StaffingPossiblityType staffingPossiblityType)
		{
			var period = getCurrentWeekPeriod(startDate);
			switch (staffingPossiblityType)
			{
				case StaffingPossiblityType.Absence:
					return
						createPeriodStaffingPossibilityViewModels(
							_scheduleStaffingPossibilityCalculator.CalculateIntradayAbsenceIntervalPossibilities(period));
				case StaffingPossiblityType.Overtime:
					return
						createPeriodStaffingPossibilityViewModels(
							_scheduleStaffingPossibilityCalculator.CalculateIntradayOvertimeIntervalPossibilities(period));
			}
			return new PeriodStaffingPossibilityViewModel[] { };
		}

		private IEnumerable<PeriodStaffingPossibilityViewModel> createPeriodStaffingPossibilityViewModels(
			IDictionary<DateOnly, IDictionary<DateTime, int>> intervalPossibilityDictionary)
		{
			var intervalLengthInMinutes = _intervalLengthFetcher.IntervalLength;
			var periodStaffingPossibilityViewModels = new List<PeriodStaffingPossibilityViewModel>();
			foreach (var intervalPossibilityItem in intervalPossibilityDictionary)
			{
				periodStaffingPossibilityViewModels.AddRange(intervalPossibilityItem.Value
					.Select(p => new PeriodStaffingPossibilityViewModel
					{
						Date = intervalPossibilityItem.Key,
						StartTime = p.Key,
						EndTime = p.Key.AddMinutes(intervalLengthInMinutes),
						Possibility = p.Value
					}).OrderBy(x => x.StartTime));
			}
			return periodStaffingPossibilityViewModels;
		}

		private DateOnlyPeriod getCurrentWeekPeriod(DateOnly date)
		{
			var lastDateInWeek = DateHelper.GetLastDateInWeek(date.Date, _loggedOnUser.CurrentUser().PermissionInformation.UICulture());
			return new DateOnlyPeriod(date, new DateOnly(lastDateInWeek));
		}
	}
}