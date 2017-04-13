using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
// ReSharper disable PossibleLossOfFraction

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable 
{
	public class SkillDayConfigurable : IDataSetup
	{
		public string Skill { get; set; }
		public string Scenario { get; set; }
		public DateOnly DateOnly { get; set; }
		public double DefaultDemand { get; set; }
		public double Shrinkage { get; set; }
		public Tuple<int, double> HourDemand { get; set; }
		public TimePeriod? OpenHours { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var skillRepository = new SkillRepository(currentUnitOfWork);
			var skill = skillRepository.LoadAllSkills().Single(x => x.Name == Skill);
			var scenarioRepository = new ScenarioRepository(currentUnitOfWork);
			var scenario = scenarioRepository.LoadAll().Single(x => x.Description.Name == Scenario);

			var defaultDemandTimespan = TimeSpan.FromMinutes(DefaultDemand * skill.DefaultResolution * (TimeSpan.FromHours(1).Ticks/TimeSpan.FromMinutes(skill.DefaultResolution).Ticks));

			if (HourDemand == null)
			{
				HourDemand = new Tuple<int, double>(0, DefaultDemand);
			}
			var hourDemandTimespan = TimeSpan.FromMinutes(HourDemand.Item2 * skill.DefaultResolution * (TimeSpan.FromHours(1).Ticks / TimeSpan.FromMinutes(skill.DefaultResolution).Ticks));

		    ISkillDay skillDay;
			skillDay = !OpenHours.HasValue ? 
								skill.CreateSkillDayWithDemandPerHour(scenario, DateOnly,defaultDemandTimespan, new Tuple<int, TimeSpan>(HourDemand.Item1, hourDemandTimespan)) 
											:
								skill.CreateSkillDayWithDemandPerHour(scenario, DateOnly, defaultDemandTimespan, new Tuple<int, TimeSpan>(HourDemand.Item1, hourDemandTimespan),OpenHours.Value );
			skillDay.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(Shrinkage));
			var skillDayRepository = new SkillDayRepository(currentUnitOfWork);
			skillDayRepository.Add(skillDay);
		}
	}
}
