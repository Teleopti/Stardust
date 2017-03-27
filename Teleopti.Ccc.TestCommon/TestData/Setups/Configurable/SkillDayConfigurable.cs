using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable 
{
	public class SkillDayConfigurable : IDataSetup
	{
		public string Skill { get; set; }
		public string Scenario { get; set; }
		public DateOnly DateOnly { get; set; }
		public double Demand { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var skillRepository = new SkillRepository(currentUnitOfWork);
			var skill = skillRepository.LoadAllSkills().Single(x => x.Name == Skill);
			var scenarioRepository = new ScenarioRepository(currentUnitOfWork);
			var scenario = scenarioRepository.LoadAll().Single(x => x.Description.Name == Scenario);

			var skillDay = skill.CreateSkillDayWithDemand(scenario, DateOnly, Demand);
			var skillDayRepository = new SkillDayRepository(currentUnitOfWork);
			skillDayRepository.Add(skillDay);
		}
	}
}
