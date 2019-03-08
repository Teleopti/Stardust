using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Staffing;
using Teleopti.Ccc.Web.Areas.MyTime.Models.ScheduleStaffingPossibility;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class StaffingPossibilityViewModelFactory : IStaffingPossibilityViewModelFactory
	{
		private readonly IAbsenceStaffingPossibilityCalculator _absenceStaffingPossibilityCalculator;
		private readonly IOvertimeStaffingPossibilityCalculator _overtimeStaffingPossibilityCalculator;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IStaffingDataAvailablePeriodProvider _staffingDataAvailablePeriodProvider;

		public StaffingPossibilityViewModelFactory(
			IStaffingDataAvailablePeriodProvider staffingDataAvailablePeriodProvider,
			IAbsenceStaffingPossibilityCalculator absenceStaffingPossibilityCalculator,
			IOvertimeStaffingPossibilityCalculator overtimeStaffingPossibilityCalculator, ILoggedOnUser loggedOnUser)
		{
			_staffingDataAvailablePeriodProvider = staffingDataAvailablePeriodProvider;
			_absenceStaffingPossibilityCalculator = absenceStaffingPossibilityCalculator;
			_overtimeStaffingPossibilityCalculator = overtimeStaffingPossibilityCalculator;
			_loggedOnUser = loggedOnUser;
		}

		public IEnumerable<PeriodStaffingPossibilityViewModel> CreatePeriodStaffingPossibilityViewModels(DateOnly startDate,
			StaffingPossibilityType staffingPossibilityType, bool returnOneWeekData)
		{
			if (staffingPossibilityType == StaffingPossibilityType.Absence)
			{
				return getAbsencePeriodStaffingPossibilityViewModels(startDate, returnOneWeekData);
			}
			if (staffingPossibilityType == StaffingPossibilityType.Overtime)
			{
				return getOvertimePeriodStaffingPossibilityViewModels(startDate, returnOneWeekData);
			}
			return emptyResult();
		}

		private IEnumerable<PeriodStaffingPossibilityViewModel> getOvertimePeriodStaffingPossibilityViewModels(DateOnly startDate, bool returnOneWeekData)
		{
			var currentUser = _loggedOnUser.CurrentUser();
			var periods = _staffingDataAvailablePeriodProvider.GetPeriodsForOvertime(currentUser, startDate, returnOneWeekData);
			if (periods.Any())
			{
				var possibilityModels = new List<CalculatedPossibilityModel>();
				var satisfyAllSkills = false;
				foreach (var period in periods)
				{
					possibilityModels.AddRange(
						_overtimeStaffingPossibilityCalculator.CalculateIntradayIntervalPossibilities(currentUser, period, satisfyAllSkills));
				}
				return createPeriodStaffingPossibilityViewModels(possibilityModels);
			}
			return emptyResult();
		}

		private IEnumerable<PeriodStaffingPossibilityViewModel> getAbsencePeriodStaffingPossibilityViewModels(DateOnly startDate, bool returnOneWeekData)
		{
			var currentUser = _loggedOnUser.CurrentUser();
			var period = _staffingDataAvailablePeriodProvider.GetPeriodForAbsence(currentUser, startDate, returnOneWeekData);
			if (period.HasValue)
			{
				var possibilityModels = _absenceStaffingPossibilityCalculator.CalculateIntradayIntervalPossibilities(currentUser, period.Value);
				return createPeriodStaffingPossibilityViewModels(possibilityModels.Models);
			}
			return emptyResult();
		}

		private IEnumerable<PeriodStaffingPossibilityViewModel> createPeriodStaffingPossibilityViewModels(
			IEnumerable<CalculatedPossibilityModel> calculatedPossibilityModels)
		{
			var periodStaffingPossibilityViewModels = new List<PeriodStaffingPossibilityViewModel>();
			foreach (var calculatedPossibilityModel in calculatedPossibilityModels)
			{
				var keys = calculatedPossibilityModel.IntervalPossibilies.Keys.OrderBy(x => x).ToArray();
				for (var i = 0; i < keys.Length; i++)
				{
					var startTime = keys[i];
					var endTime = default(DateTime);
					if (i < keys.Length - 1)
					{
						var nextStartTime = keys[i + 1];
						endTime = nextStartTime;
					}
					else
					{
						endTime = startTime.AddMinutes(calculatedPossibilityModel.Resolution);
					}

					periodStaffingPossibilityViewModels.Add(new PeriodStaffingPossibilityViewModel
					{
						Date = calculatedPossibilityModel.Date.ToFixedClientDateOnlyFormat(),
						StartTime = startTime,
						EndTime = endTime,
						Possibility = calculatedPossibilityModel.IntervalPossibilies[keys[i]]
					});
				}
			}
			return periodStaffingPossibilityViewModels;
		}

		private static IEnumerable<PeriodStaffingPossibilityViewModel> emptyResult()
		{
			return new PeriodStaffingPossibilityViewModel[] { };
		}
	}
}