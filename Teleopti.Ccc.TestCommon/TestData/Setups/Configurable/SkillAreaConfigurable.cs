using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class SkillAreaConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string Skill { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{

			var skillRepository = new SkillRepository(currentUnitOfWork);
			var skill = skillRepository.LoadAll().Single(x => x.Name.Equals(Skill));

			var skillInIntraday = new SkillInIntraday() {Id = skill.Id.Value, IsDeleted = false, Name = skill.Name};
			
			var skillArea = new SkillArea { Name = Name, Skills = new Collection<SkillInIntraday> {skillInIntraday} };

			var skillAreaRepository = new SkillAreaRepository(currentUnitOfWork);
			skillAreaRepository.Add(skillArea);
		}
	}
}