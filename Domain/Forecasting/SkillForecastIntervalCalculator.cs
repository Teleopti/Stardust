using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday.ApplicationLayer;
using Teleopti.Ccc.Domain.Intraday.To_Staffing;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public class SkillForecastIntervalCalculator
	{
		private ISkillDayRepository _skillDayRepository;
		private readonly ILoadSkillDaysWithPeriodFlexibility _loadSkillDaysWithPeriodFlexibility;
		private readonly IScenarioRepository _scenarioRepository;
		private ISkillForecastReadModelRepository _skillForecastReadModelRepository;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;

		public SkillForecastIntervalCalculator(ISkillDayRepository skillDayRepository, ILoadSkillDaysWithPeriodFlexibility loadSkillDaysWithPeriodFlexibility, IScenarioRepository scenarioRepository, ISkillForecastReadModelRepository skillForecastReadModelRepository, IIntervalLengthFetcher intervalLengthFetcher)
		{
			_skillDayRepository = skillDayRepository;
			_loadSkillDaysWithPeriodFlexibility = loadSkillDaysWithPeriodFlexibility;
			_scenarioRepository = scenarioRepository;
			_skillForecastReadModelRepository = skillForecastReadModelRepository;
			_intervalLengthFetcher = intervalLengthFetcher;
		}

		public void Calculate(List<ISkill> skills, DateOnlyPeriod dtp)
		{
			//no child skill should be filterd out
			//call this CalculateForecastedAgentsForEmailSkills

			var skillDaysBySkills =
				_loadSkillDaysWithPeriodFlexibility.Load(dtp, skills, _scenarioRepository.LoadDefaultScenario());
			var skillDays = skillDaysBySkills.SelectMany(x => x.Value);
			var periods = skillDays
				.SelectMany(x =>
					x.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(_intervalLengthFetcher.GetIntervalLength()), false)
						.Select(i => new {SkillDay = x, StaffPeriod = i}));
			var intervals = periods
				.Where(x => x.StaffPeriod.Period.StartDateTime >= dtp.StartDate.Date && x.StaffPeriod.Period.EndDateTime <= dtp.EndDate.Date)
				.Select(x => new SkillForecast
				{
					SkillId = x.SkillDay.Skill.Id.Value,
					StartDateTime = x.StaffPeriod.Period.StartDateTime,
					EndDateTime = x.StaffPeriod.Period.EndDateTime,
					Agents = x.StaffPeriod.FStaff,
					Calls = x.StaffPeriod.ForecastedTasks,
					AverageHandleTime = x.StaffPeriod.AverageHandlingTaskTime.TotalSeconds
				}).ToList();
			_skillForecastReadModelRepository.PersistSkillForecast(intervals);
		}
	}
}