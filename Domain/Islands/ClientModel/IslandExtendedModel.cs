using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Islands.ClientModel
{
	public class IslandExtendedModel
	{
		public IEnumerable<ISkill> SkillsInIsland { get; set; }
		public IEnumerable<SkillSetExtendedModel> SkillSetsInIsland { get; set; }
	}

	public class SkillSetExtendedModel
	{
		public IEnumerable<ISkill> SkillsInSkillSet { get; set; }
		public IEnumerable<IPerson> AgentsInSkillSet { get; set; }
	}
}