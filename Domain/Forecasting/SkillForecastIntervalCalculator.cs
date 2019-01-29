using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Intraday.ApplicationLayer;
using Teleopti.Ccc.Domain.Intraday.To_Staffing;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public class SkillForecastIntervalCalculator
	{
		private readonly ILoadSkillDaysWithPeriodFlexibility _loadSkillDaysWithPeriodFlexibility;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ISkillForecastReadModelRepository _skillForecastReadModelRepository;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;

		public SkillForecastIntervalCalculator(ILoadSkillDaysWithPeriodFlexibility loadSkillDaysWithPeriodFlexibility,
			IScenarioRepository scenarioRepository, ISkillForecastReadModelRepository skillForecastReadModelRepository,
			IIntervalLengthFetcher intervalLengthFetcher)
		{
			_loadSkillDaysWithPeriodFlexibility = loadSkillDaysWithPeriodFlexibility;
			_scenarioRepository = scenarioRepository;
			_skillForecastReadModelRepository = skillForecastReadModelRepository;
			_intervalLengthFetcher = intervalLengthFetcher;
		}

		//remove this method later on
		public void Calculate(List<ISkill> skills, DateOnlyPeriod dtp)
		{
			//no child skill should be filterd out
			//call this CalculateForecastedAgentsForEmailSkills

			var skillDaysBySkills =
				_loadSkillDaysWithPeriodFlexibility.Load(dtp, skills, _scenarioRepository.LoadDefaultScenario());
			var skillDays = skillDaysBySkills.SelectMany(x => x.Value);
			Calculate(skillDays);
		}

		//this method will be the one later on we will remove the other method TDD

		public void Calculate(IEnumerable<ISkillDay> skillDays)
		{
			var periods = skillDays
				.SelectMany(x =>
					x.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(_intervalLengthFetcher.GetIntervalLength()), false)
						.Select(i => new { SkillDay = x, StaffPeriod = i }));
			var periodsWithShrinkage = skillDays
				.SelectMany(x =>
					x.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(_intervalLengthFetcher.GetIntervalLength()), true)
						.Select(i => new { SkillDay = x, StaffPeriod = i }));

			var agentsWithShrinkage = periodsWithShrinkage.ToDictionary(
				x => new { SkillId = x.SkillDay.Skill.Id.GetValueOrDefault(), StartDateTime = x.StaffPeriod.Period.StartDateTime },
				y => y.StaffPeriod.FStaff);

			var result = new List<SkillForecast>();
			periods.ForEach(x =>
			{
				var skillId = x.SkillDay.Skill.Id.GetValueOrDefault();
				var startDateTime = x.StaffPeriod.Period.StartDateTime;
				var item = new { SkillId = skillId, StartDateTime = startDateTime };
				result.Add(new SkillForecast
				{
					SkillId = skillId,
					StartDateTime = startDateTime,
					EndDateTime = x.StaffPeriod.Period.EndDateTime,
					Agents = x.StaffPeriod.FStaff,
					Calls = x.StaffPeriod.ForecastedTasks,
					AverageHandleTime = x.StaffPeriod.AverageHandlingTaskTime.TotalSeconds,
					AgentsWithShrinkage = agentsWithShrinkage.ContainsKey(item) ? agentsWithShrinkage[item] : 0,
					IsBackOffice = SkillTypesWithBacklog.IsBacklogSkillType(x.SkillDay.Skill)

				});

			});

			_skillForecastReadModelRepository.PersistSkillForecast(result);
		}
	}
}