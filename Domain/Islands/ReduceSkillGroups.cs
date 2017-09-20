using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class ReduceSkillGroups : IReduceSkillGroups
	{
		private readonly ReduceIslandsLimits _reduceIslandsLimits;

		public ReduceSkillGroups(ReduceIslandsLimits reduceIslandsLimits)
		{
			_reduceIslandsLimits = reduceIslandsLimits;
		}

		public bool Execute(IEnumerable<IEnumerable<SkillSet>> islands, IDictionary<ISkill, int> noAgentsKnowingSkill)
		{
			var success = false;
		
			foreach (var island in islands)
			{
				var removed = 0;
				var numberOfAgentsInIsland = island.Sum(x => x.Agents.Count());
				if (numberOfAgentsInIsland < _reduceIslandsLimits.MinimumNumberOfAgentsInIsland)
					continue;
				var skillGroupsOnIslandAndNumberOfAgents = island.Select(x => new { SkillGroup = x, NumberOfAgentsOnSkillGroup = x.Agents.Count() }).OrderByDescending(x => x.NumberOfAgentsOnSkillGroup);
				foreach (var skillGroupAndNumberOfAgents in skillGroupsOnIslandAndNumberOfAgents)
				{
					foreach (var skillGroupSkill in skillGroupAndNumberOfAgents.SkillGroup.Skills.ToArray())
					{
						if (skillGroupAndNumberOfAgents.NumberOfAgentsOnSkillGroup * _reduceIslandsLimits.MinimumFactorOfAgentsInOtherSkillGroup(numberOfAgentsInIsland) >= noAgentsKnowingSkill[skillGroupSkill])
							continue;

						if (skillGroupAndNumberOfAgents.SkillGroup.Skills.Count(x => x.Activity == null || x.Activity.Equals(skillGroupSkill.Activity)) < 2)
							continue;

						skillGroupAndNumberOfAgents.SkillGroup.RemoveSkill(skillGroupSkill);
						noAgentsKnowingSkill[skillGroupSkill] -= skillGroupAndNumberOfAgents.NumberOfAgentsOnSkillGroup;

						removed += skillGroupAndNumberOfAgents.NumberOfAgentsOnSkillGroup;
						if (numberOfAgentsInIsland - removed < _reduceIslandsLimits.MinimumNumberOfAgentsInIsland)
							return true;
						success = true;
					}
				}
			}
			return success;
		}
	}
}