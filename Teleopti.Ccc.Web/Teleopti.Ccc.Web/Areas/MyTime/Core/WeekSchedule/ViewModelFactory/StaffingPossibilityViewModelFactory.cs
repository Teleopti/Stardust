using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Models.ScheduleStaffingPossibility;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class StaffingPossibilityViewModelFactory : IStaffingPossibilityViewModelFactory
	{
		private readonly IScheduleStaffingPossibilityCalculator _scheduleStaffingPossibilityCalculator;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;

		public StaffingPossibilityViewModelFactory(
			IScheduleStaffingPossibilityCalculator scheduleStaffingPossibilityCalculator, IIntervalLengthFetcher intervalLengthFetcher)
		{
			_scheduleStaffingPossibilityCalculator = scheduleStaffingPossibilityCalculator;
			_intervalLengthFetcher = intervalLengthFetcher;
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
			return possibilities;
		}
	}
}