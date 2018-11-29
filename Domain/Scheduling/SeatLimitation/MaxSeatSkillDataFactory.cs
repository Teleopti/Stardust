using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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

		public MaxSeatSkillData Create(DateOnlyPeriod period, IEnumerable<IPerson> agents, IScenario scenario,IEnumerable<IPerson> allAgents, int intervalLength)
		{
			var ret = new MaxSeatSkillData();
			var sites = _maxSeatSitesExtractor.MaxSeatSites(period, agents);
			foreach (var site in sites)
			{
				var skill = _skillsFromMaxSeatSite.CreateMaxSeatSkill(site, intervalLength);
				foreach (var agent in allAgents)
				{
					foreach (var personPeriod in agent.PersonPeriods(period))
					{
						if (personPeriod.Team.Site.Equals(site))
						{
							personPeriod.SetMaxSeatSkill(skill);
						}
					}
				}
				var skillDays = createMaxSeatSkillDays(period, skill, scenario);
				ret.Add(skill, skillDays, site);
			}
			return ret;
		}

		private IEnumerable<ISkillDay> createMaxSeatSkillDays(DateOnlyPeriod period, MaxSeatSkill maxSeatSkill, IScenario scenario)
		{
			var periodForTimeZoneAndNightShifts = period.Inflate(1);
			var skillDays = skillDaysForSkill(periodForTimeZoneAndNightShifts, maxSeatSkill, scenario);
			foreach (var skillDay in skillDays)
			{
				skillDay.OpenAllSkillStaffPeriods(maxSeatSkill.MaxSeats);
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