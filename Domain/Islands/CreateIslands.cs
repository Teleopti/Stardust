using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class CreateIslands
	{
		private readonly CreateSkillGroups _createSkillGroups;
		private readonly NumberOfAgentsKnowingSkill _numberOfAgentsKnowingSkill;
		private readonly MoveSkillGroupToCorrectIsland _moveSkillGroupToCorrectIsland;

		public CreateIslands(CreateSkillGroups createSkillGroups, 
								NumberOfAgentsKnowingSkill numberOfAgentsKnowingSkill,
								MoveSkillGroupToCorrectIsland moveSkillGroupToCorrectIsland)
		{
			_createSkillGroups = createSkillGroups;
			_numberOfAgentsKnowingSkill = numberOfAgentsKnowingSkill;
			_moveSkillGroupToCorrectIsland = moveSkillGroupToCorrectIsland;
		}

		public IEnumerable<Island> Create(IReduceSkillGroups reduceSkillGroups, IEnumerable<IPerson> peopleInOrganization, DateOnlyPeriod period)
		{
			var allSkillGroups = _createSkillGroups.Create(peopleInOrganization, period.StartDate).ToList();
			var noAgentsKnowingSkill = _numberOfAgentsKnowingSkill.Execute(allSkillGroups);
			while (true)
			{
				var skillGroupsInIslands = allSkillGroups.Select(skillGroup => new List<SkillSet> { skillGroup }).ToList();
				while (_moveSkillGroupToCorrectIsland.Execute(allSkillGroups, skillGroupsInIslands))
				{
					removeEmptyIslands(skillGroupsInIslands);
				}

				if (!reduceSkillGroups.Execute(skillGroupsInIslands, noAgentsKnowingSkill))
				{
					return skillGroupsInIslands.Select(skillGroupInIsland => new Island(skillGroupInIsland, noAgentsKnowingSkill)).ToArray();
				}
			}
		}

		private static void removeEmptyIslands(ICollection<List<SkillSet>> skillGroupsInIslands)
		{
			skillGroupsInIslands.Where(x => x.Count == 0).ToArray().ForEach(x => skillGroupsInIslands.Remove(x));
		}
	}
}