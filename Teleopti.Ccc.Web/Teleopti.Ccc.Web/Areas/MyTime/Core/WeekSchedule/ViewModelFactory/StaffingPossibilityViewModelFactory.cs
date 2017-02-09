using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Models.ScheduleStaffingPossibility;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class StaffingPossibilityViewModelFactory : IStaffingPossibilityViewModelFactory
	{
		private const int intervalLengthInMinutes = 15;
		private readonly IScheduleStaffingPossibilityCalculator _scheduleStaffingPossibilityCalculator;

		public StaffingPossibilityViewModelFactory(
			IScheduleStaffingPossibilityCalculator scheduleStaffingPossibilityCalculator)
		{
			_scheduleStaffingPossibilityCalculator = scheduleStaffingPossibilityCalculator;
		}

		public IEnumerable<PeriodStaffingPossibilityViewModel> CreateIntradayPeriodStaffingPossibilityViewModels(
			StaffingPossiblity staffingPossiblity)
		{
			switch (staffingPossiblity)
			{
				case StaffingPossiblity.Absence:
					return
						createPeriodStaffingPossibilityViewModels(
							_scheduleStaffingPossibilityCalculator.CalcuateIntradayAbsenceIntervalPossibilities());
				case StaffingPossiblity.Overtime:
					return
						createPeriodStaffingPossibilityViewModels(
							_scheduleStaffingPossibilityCalculator.CalcuateIntradayOvertimeIntervalPossibilities());
			}
			return new PeriodStaffingPossibilityViewModel[] {};
		}

		private IEnumerable<PeriodStaffingPossibilityViewModel> createPeriodStaffingPossibilityViewModels(
			IDictionary<DateTime, int> intervalPossibilities)
		{
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