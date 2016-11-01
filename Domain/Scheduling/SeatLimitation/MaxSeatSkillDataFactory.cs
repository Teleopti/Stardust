using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class MaxSeatSkillDataFactory
	{
		private readonly MaxSeatSitesExtractor _maxSeatSitesExtractor;
		private readonly SkillsFromMaxSeatSite _skillsFromMaxSeatSite;
		private readonly IWorkloadDayHelper _workloadDayHelper;


		public MaxSeatSkillDataFactory(MaxSeatSitesExtractor maxSeatSitesExtractor, 
							SkillsFromMaxSeatSite skillsFromMaxSeatSite, 
							IWorkloadDayHelper workloadDayHelper)
		{
			_maxSeatSitesExtractor = maxSeatSitesExtractor;
			_skillsFromMaxSeatSite = skillsFromMaxSeatSite;
			_workloadDayHelper = workloadDayHelper;
		}

		public MaxSeatSkillData Create(DateOnlyPeriod period, IEnumerable<IPerson> agents, IScenario scenario,IEnumerable<IPerson> allAgents)
		{
			var ret = new MaxSeatSkillData();
			var sites = _maxSeatSitesExtractor.MaxSeatSites(period, agents);
			foreach (var site in sites)
			{
				var skill = _skillsFromMaxSeatSite.CreateMaxSeatSkill(site, 15);//TODO: why 15?

				//todo: REMOVE ME?
				foreach (var agent in allAgents)
				{
					var personSkill = new PersonSkill(skill, new Percent(1));
					agent.Period(period.StartDate).AddPersonMaxSeatSkill(personSkill);
				}
				//

				var skillDays = createMaxSeatSkillDays(period, skill, scenario);
				ret.Add(skill, skillDays);
			}
			return ret;
		}

		private IEnumerable<ISkillDay> createMaxSeatSkillDays(DateOnlyPeriod period, ISkill maxSeatSkill, IScenario scenario)
		{
			var skillDays = skillDaysForSkill(period, maxSeatSkill, scenario);
			foreach (IMaxSeatSkillDay skillDay in skillDays)
			{
				skillDay.OpenAllSkillStaffPeriods();
			}
			return skillDays;
		}

		private IEnumerable<ISkillDay> skillDaysForSkill(DateOnlyPeriod period, ISkill skill, IScenario scenario)
		{
			var skillDays = new List<ISkillDay>();

			foreach (var date in period.DayCollection())
			{
				var skillDay = new SkillDay();
				skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new List<ISkillDay> { skillDay }, new DateOnlyPeriod());
				var skillDayTemplate = skill.GetTemplateAt((int)date.DayOfWeek);

				skillDay.CreateFromTemplate(date, skill, scenario, skillDayTemplate);

				skillDays.Add(skillDay);
			}

			_workloadDayHelper.CreateLongtermWorkloadDays(skill, skillDays);
			return skillDays;
		}
	}
}