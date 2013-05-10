﻿using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class DailyStaffingMetricsViewModelFactory : IDailyStaffingMetricsViewModelFactory
	{
		private readonly ISkillRepository _skillRepository;
		private readonly IResourceCalculateSkillCommand _calculateSkillCommand;
		private readonly ICurrentScenario _currentScenario;
		private readonly ISchedulingResultStateHolder _stateHolder;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;

		public DailyStaffingMetricsViewModelFactory(ISkillRepository skillRepository, IResourceCalculateSkillCommand calculateSkillCommand, ICurrentScenario currentScenario, ISchedulingResultStateHolder stateHolder, IResourceOptimizationHelper resourceOptimizationHelper)
		{
			_skillRepository = skillRepository;
			_calculateSkillCommand = calculateSkillCommand;
			_currentScenario = currentScenario;
			_stateHolder = stateHolder;
			_resourceOptimizationHelper = resourceOptimizationHelper;
		}

		public DailyStaffingMetricsViewModel CreateViewModel(Guid skillId, DateTime date)
		{
			var skill = _skillRepository.Load(skillId);
			var dateOnly = new DateOnly(date);
			
			_calculateSkillCommand.Execute(_currentScenario.Current(), new DateOnlyPeriod(dateOnly,dateOnly).ToDateTimePeriod(skill.TimeZone), skill);

			_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, true);

			var skillDay = _stateHolder.SkillDays[skill].First(d => d.CurrentDate == dateOnly);
			var sumOfForecastedHours = skillDay.ForecastedIncomingDemand;
			var estimatedServiceLevel = SkillStaffPeriodHelper.EstimatedServiceLevel(skillDay.SkillStaffPeriodCollection).Value;
			var relativeDifference = SkillStaffPeriodHelper.RelativeDifferenceForDisplay(skillDay.SkillStaffPeriodCollection);
			var absoluteDifference = SkillStaffPeriodHelper.AbsoluteDifference(skillDay.SkillStaffPeriodCollection, false, false);
			var scheduledHours = SkillStaffPeriodHelper.ScheduledHours(skillDay.SkillStaffPeriodCollection);
			return new DailyStaffingMetricsViewModel
				{
					ForecastedHours = sumOfForecastedHours.TotalHours,
					ESL = estimatedServiceLevel,
					ScheduledHours = scheduledHours,
					RelativeDifference = relativeDifference,
					AbsoluteDifferenceHours = absoluteDifference == null ? (double?) null : absoluteDifference.Value.TotalHours
				};
		}
	}
}