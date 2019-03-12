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

		public IEnumerable<PeriodStaffingPossibilityViewModel> CreatePeriodStaffingPossibilityViewModelsForWeek(DateOnly startDate,
			StaffingPossibilityType staffingPossibilityType)
		{
			if (staffingPossibilityType == StaffingPossibilityType.Absence)
			{
				var periodForAbsence = _staffingDataAvailablePeriodProvider.GetPeriodForAbsenceForWeek(_loggedOnUser.CurrentUser(), startDate);

				if (periodForAbsence.HasValue)
				{
					return getAbsencePeriodStaffingPossibilityViewModels(periodForAbsence.Value);
				}
			}

			if (staffingPossibilityType == StaffingPossibilityType.Overtime)
			{
				var periodsForOvertime = _staffingDataAvailablePeriodProvider.GetPeriodsForOvertimeForWeek(_loggedOnUser.CurrentUser(), startDate);

				if (periodsForOvertime.Any())
				{
					return getOvertimePeriodStaffingPossibilityViewModels(periodsForOvertime);
				}
			}

			return emptyResult();
		}

		public IEnumerable<PeriodStaffingPossibilityViewModel> CreatePeriodStaffingPossibilityViewModelsForMobileDay(
			DateOnly startDate,
			StaffingPossibilityType staffingPossibilityType)
		{
			if (staffingPossibilityType == StaffingPossibilityType.Absence)
			{
				var period =
					_staffingDataAvailablePeriodProvider.GetPeriodForAbsenceForMobileDay(_loggedOnUser.CurrentUser(),
						startDate);

				if (period.HasValue)
				{
					return getAbsencePeriodStaffingPossibilityViewModels(period.Value);
				}
			}

			if (staffingPossibilityType == StaffingPossibilityType.Overtime)
			{
				var periodsForOvertime = _staffingDataAvailablePeriodProvider.GetPeriodsForOvertimeForMobileDay(_loggedOnUser.CurrentUser(), startDate);

				if (periodsForOvertime.Any())
				{
					return getOvertimePeriodStaffingPossibilityViewModels(periodsForOvertime);
				}
			}


			return emptyResult();
		}

		private IEnumerable<PeriodStaffingPossibilityViewModel> getAbsencePeriodStaffingPossibilityViewModels(
			DateOnlyPeriod period)
		{
			var possibilityModels =
				_absenceStaffingPossibilityCalculator.CalculateIntradayIntervalPossibilities(_loggedOnUser.CurrentUser(), period);
			return createPeriodStaffingPossibilityViewModels(possibilityModels.Models);
		}

		private IEnumerable<PeriodStaffingPossibilityViewModel> getOvertimePeriodStaffingPossibilityViewModels(
			List<DateOnlyPeriod> periods)
		{
			var possibilityModels = new List<CalculatedPossibilityModel>();
			foreach (var period in periods)
			{
				possibilityModels.AddRange(
					_overtimeStaffingPossibilityCalculator.CalculateIntradayIntervalPossibilities(
						_loggedOnUser.CurrentUser(), period, false));
			}

			return createPeriodStaffingPossibilityViewModels(possibilityModels);
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