using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands.ClientModel
{
	public class IslandExtendedModel
	{
		public IEnumerable<ISkill> SkillsInIsland { get; set; }
		public IEnumerable<SkillGroupExtendedModel> SkillGroupsInIsland { get; set; }
	}

	public class SkillGroupExtendedModel
	{
		public IEnumerable<ISkill> SkillsInSkillGroup { get; set; }
		public IEnumerable<IPerson> AgentsInSkillGroup { get; set; }
	}
}