﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Models.ScheduleStaffingPossibility;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class StaffingPossibilityViewModelFactory : IStaffingPossibilityViewModelFactory
	{
		private readonly IScheduleStaffingPossibilityCalculator _scheduleStaffingPossibilityCalculator;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly INow _now;
		private readonly IToggleManager _toggleManager;
		private readonly int _maxAvailableDays = 13;

		public StaffingPossibilityViewModelFactory(
			IScheduleStaffingPossibilityCalculator scheduleStaffingPossibilityCalculator, ILoggedOnUser loggedOnUser, INow now, IToggleManager toggleManager)
		{
			_scheduleStaffingPossibilityCalculator = scheduleStaffingPossibilityCalculator;
			_loggedOnUser = loggedOnUser;
			_now = now;
			_toggleManager = toggleManager;
		}

		public IEnumerable<PeriodStaffingPossibilityViewModel> CreatePeriodStaffingPossibilityViewModels(DateOnly startDate,
			StaffingPossiblityType staffingPossiblityType)
		{
			var period = getAvailablePeriod(startDate);
			if (!period.HasValue) return new PeriodStaffingPossibilityViewModel[] {};
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
			return new PeriodStaffingPossibilityViewModel[] {};
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

		private DateOnlyPeriod? getAvailablePeriod(DateOnly date)
		{
			var culture = _loggedOnUser.CurrentUser().PermissionInformation.UICulture();
			var weekPeriod = DateHelper.GetWeekPeriod(date, culture);
			var today = _now.LocalDateOnly();
			var maxEndDate = today;
			if (_toggleManager.IsEnabled(Domain.FeatureFlags.Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880))
			{
				maxEndDate = today.AddDays(_maxAvailableDays);
			}
			var availablePeriod = new DateOnlyPeriod(today, maxEndDate);
			return availablePeriod.Intersection(weekPeriod);
		}
	}
}