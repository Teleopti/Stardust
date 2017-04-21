using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Web.Areas.MyTime.Models.ScheduleStaffingPossibility;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class StaffingPossibilityViewModelFactory : IStaffingPossibilityViewModelFactory
	{
		private readonly IScheduleStaffingPossibilityCalculator _scheduleStaffingPossibilityCalculator;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly INow _now;

		public StaffingPossibilityViewModelFactory(
			IScheduleStaffingPossibilityCalculator scheduleStaffingPossibilityCalculator, ILoggedOnUser loggedOnUser, INow now)
		{
			_scheduleStaffingPossibilityCalculator = scheduleStaffingPossibilityCalculator;
			_loggedOnUser = loggedOnUser;
			_now = now;
		}

		public IEnumerable<PeriodStaffingPossibilityViewModel> CreatePeriodStaffingPossibilityViewModels(DateOnly startDate,
			StaffingPossiblityType staffingPossiblityType)
		{
			var period = getAvailablePeriod(startDate);
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
			IEnumerable<CalculatedPossibilityModel> calculatedPossibilityModels)
		{
			var periodStaffingPossibilityViewModels = new List<PeriodStaffingPossibilityViewModel>();
			foreach (var calculatedPossibilityModel in calculatedPossibilityModels)
			{
				periodStaffingPossibilityViewModels.AddRange(calculatedPossibilityModel.IntervalPossibilies
					.Select(p => new PeriodStaffingPossibilityViewModel
					{
						Date = calculatedPossibilityModel.Date,
						StartTime = p.Key,
						EndTime = p.Key.AddMinutes(calculatedPossibilityModel.Resolution),
						Possibility = p.Value
					}).OrderBy(x => x.StartTime));
			}
			return periodStaffingPossibilityViewModels;
		}

		private DateOnlyPeriod getAvailablePeriod(DateOnly date)
		{
			var culture = _loggedOnUser.CurrentUser().PermissionInformation.UICulture();
			var period = DateHelper.GetWeekPeriod(date, culture);
			//if (period.StartDate > new DateOnly(_now.LocalDa))
			//{
				
			//}
			//var lastDateInWeek = DateHelper.GetLastDateInWeek(date.Date,
			//	_loggedOnUser.CurrentUser().PermissionInformation.UICulture());
			//return new DateOnlyPeriod(date, new DateOnly(lastDateInWeek));
			return period;
		}
	}
}