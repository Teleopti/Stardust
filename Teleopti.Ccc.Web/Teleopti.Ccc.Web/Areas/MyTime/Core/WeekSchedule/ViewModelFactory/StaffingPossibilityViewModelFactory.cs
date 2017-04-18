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

		public IEnumerable<PeriodStaffingPossibilityViewModel> CreateIntradayPeriodStaffingPossibilityViewModels(
			StaffingPossiblityType staffingPossiblityType)
		{
			switch (staffingPossiblityType)
			{
				case StaffingPossiblityType.Absence:
					return
						createPeriodStaffingPossibilityViewModels(
							_scheduleStaffingPossibilityCalculator.CalcuateIntradayAbsenceIntervalPossibilities());
				case StaffingPossiblityType.Overtime:
					return
						createPeriodStaffingPossibilityViewModels(
							_scheduleStaffingPossibilityCalculator.CalcuateIntradayOvertimeIntervalPossibilities());
			}
			return new PeriodStaffingPossibilityViewModel[] { };
		}

		private IEnumerable<PeriodStaffingPossibilityViewModel> createPeriodStaffingPossibilityViewModels(
			IDictionary<DateTime, int> intervalPossibilities)
		{
			var intervalLengthInMinutes = _intervalLengthFetcher.IntervalLength;
			var possibilities =
				intervalPossibilities.Select(intervalPossibility => new PeriodStaffingPossibilityViewModel
				{
					StartTime = intervalPossibility.Key,
					EndTime = intervalPossibility.Key.AddMinutes(intervalLengthInMinutes),
					Possibility = intervalPossibility.Value
				});
			return possibilities.OrderBy(x => x.StartTime);
		}

		//private DateOnlyPeriod getCurrentWeekPeriod(DateOnly date)
		//{
		//	//DateHelper.GetWeekPeriod(date);
		//}
	}
}