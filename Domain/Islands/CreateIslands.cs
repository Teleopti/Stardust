using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class CreateIslands
	{
		private readonly CreateSkillSets _createSkillSets;
		private readonly NumberOfAgentsKnowingSkill _numberOfAgentsKnowingSkill;
		private readonly MoveSkillSetToCorrectIsland _moveSkillSetToCorrectIsland;

		public CreateIslands(CreateSkillSets createSkillSets, 
								NumberOfAgentsKnowingSkill numberOfAgentsKnowingSkill,
								MoveSkillSetToCorrectIsland moveSkillSetToCorrectIsland)
		{
			_createSkillSets = createSkillSets;
			_numberOfAgentsKnowingSkill = numberOfAgentsKnowingSkill;
			_moveSkillSetToCorrectIsland = moveSkillSetToCorrectIsland;
		}

		public IEnumerable<Island> Create(IReduceSkillSets reduceSkillSets, IEnumerable<IPerson> peopleInOrganization, DateOnlyPeriod period)
		{
			var allSkillSets = _createSkillSets.Create(peopleInOrganization, period.StartDate).ToList();
			var noAgentsKnowingSkill = _numberOfAgentsKnowingSkill.Execute(allSkillSets);
			while (true)
			{
				var skillSetsInIsland = allSkillSets.Select(skillSet => new List<SkillSet> { skillSet }).ToList();
				while (_moveSkillSetToCorrectIsland.Execute(allSkillSets, skillSetsInIsland))
				{
					removeEmptyIslands(skillSetsInIsland);
				}

				if (!reduceSkillSets.Execute(skillSetsInIsland, noAgentsKnowingSkill))
				{
					return skillSetsInIsland.Select(skillSetInIsland => new Island(skillSetInIsland, noAgentsKnowingSkill)).ToArray();
				}
			}
		}

		private static void removeEmptyIslands(ICollection<List<SkillSet>> skillSetsInIsland)
		{
			skillSetsInIsland.Where(x => x.Count == 0).ToArray().ForEach(x => skillSetsInIsland.Remove(x));
		}
	}
}