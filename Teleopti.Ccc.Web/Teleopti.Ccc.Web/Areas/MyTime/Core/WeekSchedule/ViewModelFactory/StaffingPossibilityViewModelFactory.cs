using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.ScheduleStaffingPossibility;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class StaffingPossibilityViewModelFactory : IStaffingPossibilityViewModelFactory
	{
		private readonly IScheduleStaffingPossibilityCalculator _scheduleStaffingPossibilityCalculator;
		private readonly IStaffingDataAvailablePeriodProvider _staffingDataAvailablePeriodProvider;

		public StaffingPossibilityViewModelFactory(
			IScheduleStaffingPossibilityCalculator scheduleStaffingPossibilityCalculator,
			IStaffingDataAvailablePeriodProvider staffingDataAvailablePeriodProvider)
		{
			_scheduleStaffingPossibilityCalculator = scheduleStaffingPossibilityCalculator;
			_staffingDataAvailablePeriodProvider = staffingDataAvailablePeriodProvider;
		}

		public IEnumerable<PeriodStaffingPossibilityViewModel> CreatePeriodStaffingPossibilityViewModels(DateOnly startDate,
			StaffingPossiblityType staffingPossiblityType, bool returnOneWeekData)
		{
			var period = _staffingDataAvailablePeriodProvider.GetPeriod(startDate, returnOneWeekData);
			if (!period.HasValue) return new PeriodStaffingPossibilityViewModel[] { };
			switch (staffingPossiblityType)
			{
				case StaffingPossiblityType.Absence:
					return
						createPeriodStaffingPossibilityViewModels(
							_scheduleStaffingPossibilityCalculator.CalculateIntradayAbsenceIntervalPossibilities(period.Value));
				case StaffingPossiblityType.Overtime:
					return
						createPeriodStaffingPossibilityViewModels(
							_scheduleStaffingPossibilityCalculator.CalculateIntradayOvertimeIntervalPossibilities(period.Value));
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
						Date = calculatedPossibilityModel.Date.ToFixedClientDateOnlyFormat(),
						StartTime = p.Key,
						EndTime = p.Key.AddMinutes(calculatedPossibilityModel.Resolution),
						Possibility = p.Value
					}).OrderBy(x => x.StartTime));
			}
			return periodStaffingPossibilityViewModels;
		}
	}
}