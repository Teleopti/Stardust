using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Intraday.To_Staffing;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public class SkillForecastIntervalCalculator
	{
		private readonly ISkillForecastReadModelRepository _skillForecastReadModelRepository;
		private readonly IStardustJobFeedback _stardustJobFeedback;

		public SkillForecastIntervalCalculator(ISkillForecastReadModelRepository skillForecastReadModelRepository,
			 IStardustJobFeedback stardustJobFeedback)
		{
			_skillForecastReadModelRepository = skillForecastReadModelRepository;
			_stardustJobFeedback = stardustJobFeedback;
		}

		public void Calculate(ICollection<ISkillDay> skillDays, IEnumerable<ISkill> skills, DateOnlyPeriod period)
		{
			_stardustJobFeedback.SendProgress($"Starting skill forecast interval calculation");
			
			var periods = skillDays
				.SelectMany(x =>
					x.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(x.Skill.DefaultResolution), false)
						.Select(i => new { SkillDay = x, StaffPeriod = i }));
			var periodsWithShrinkage = skillDays
				.SelectMany(x =>
					x.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(x.Skill.DefaultResolution), true)
						.Select(i => new { SkillDay = x, StaffPeriod = i }));

			var agentsWithShrinkage = periodsWithShrinkage.ToDictionary(
				x => new { SkillId = x.SkillDay.Skill.Id.GetValueOrDefault(), x.StaffPeriod.Period.StartDateTime },
				y => y.StaffPeriod.ForecastedIncomingDemand);

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
					Agents = x.StaffPeriod.ForecastedIncomingDemand,
					Calls = x.StaffPeriod.ForecastedTasks,
					AverageHandleTime = x.StaffPeriod.AverageHandlingTaskTime.TotalSeconds,
					AgentsWithShrinkage = agentsWithShrinkage.ContainsKey(item) ? agentsWithShrinkage[item] : 0,
					IsBackOffice = SkillTypesWithBacklog.IsBacklogSkillType(x.SkillDay.Skill),
					AnsweredWithinSeconds = x.StaffPeriod.AnsweredWithinSeconds,
					PercentAnswered = x.StaffPeriod.PercentAnswered.Value

				});

			});
			_stardustJobFeedback.SendProgress($"Persisting {result.Count} intervals for {string.Join(",",skills)}");

			_skillForecastReadModelRepository.PersistSkillForecast(result);
		}
	}
}