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
			if (staffingPossiblityType == StaffingPossiblityType.Absence)
			{
				return getAbsencePeriodStaffingPossibilityViewModels(startDate, returnOneWeekData);
			}
			if (staffingPossiblityType == StaffingPossiblityType.Overtime)
			{
				return getOvertimePeriodStaffingPossibilityViewModels(startDate, returnOneWeekData);
			}
			return emptyResult();
		}

		private IEnumerable<PeriodStaffingPossibilityViewModel> getOvertimePeriodStaffingPossibilityViewModels(DateOnly startDate, bool returnOneWeekData)
		{
			var period = _staffingDataAvailablePeriodProvider.GetPeriodForOvertime(startDate, returnOneWeekData);
			if (period.HasValue)
			{
				var possibilityModels =
					_scheduleStaffingPossibilityCalculator.CalculateIntradayOvertimeIntervalPossibilities(period.Value);
				return createPeriodStaffingPossibilityViewModels(possibilityModels);
			}
			return emptyResult();
		}

		private IEnumerable<PeriodStaffingPossibilityViewModel> getAbsencePeriodStaffingPossibilityViewModels(DateOnly startDate, bool returnOneWeekData)
		{
			var period = _staffingDataAvailablePeriodProvider.GetPeriodForAbsence(startDate, returnOneWeekData);
			if (period.HasValue)
			{
				var possibilityModels =
					_scheduleStaffingPossibilityCalculator.CalculateIntradayAbsenceIntervalPossibilities(period.Value);
				return createPeriodStaffingPossibilityViewModels(possibilityModels);
			}
			return emptyResult();
		}

		private IEnumerable<PeriodStaffingPossibilityViewModel> createPeriodStaffingPossibilityViewModels(
			IEnumerable<CalculatedPossibilityModel> calculatedPossibilityModels)
		{
			var periodStaffingPossibilityViewModels = new List<PeriodStaffingPossibilityViewModel>();
			foreach (var calculatedPossibilityModel in calculatedPossibilityModels)
			{
				var possibilities = calculatedPossibilityModel.IntervalPossibilies.Select(
					p => new PeriodStaffingPossibilityViewModel
					{
						Date = calculatedPossibilityModel.Date.ToFixedClientDateOnlyFormat(),
						StartTime = p.Key,
						EndTime = p.Key.AddMinutes(calculatedPossibilityModel.Resolution),
						Possibility = p.Value
					}).OrderBy(x => x.StartTime);
				periodStaffingPossibilityViewModels.AddRange(possibilities);
			}
			return periodStaffingPossibilityViewModels;
		}

		private static IEnumerable<PeriodStaffingPossibilityViewModel> emptyResult()
		{
			return new PeriodStaffingPossibilityViewModel[] { };
		}
	}
}