using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class ReduceSkillGroups : IReduceSkillGroups
	{
		private readonly ReduceIslandsLimits _reduceIslandsLimits;

		public ReduceSkillGroups(ReduceIslandsLimits reduceIslandsLimits)
		{
			_reduceIslandsLimits = reduceIslandsLimits;
		}

		public bool Execute(IEnumerable<IEnumerable<SkillGroup>> groupedSkillGroups, IDictionary<ISkill, int> noAgentsKnowingSkill)
		{
			foreach (var skillGroupsOnIsland in groupedSkillGroups)
			{
				var numberOfAgentsInIsland = skillGroupsOnIsland.Sum(x => x.Agents.Count());
				if (numberOfAgentsInIsland < _reduceIslandsLimits.MinimumNumberOfAgentsInIsland)
					continue;
				var skillGroupsOnIslandAndNumberOfAgents = skillGroupsOnIsland.Select(x => new { SkillGroup = x, NumberOfAgentsOnSkillGroup = x.Agents.Count() }).OrderByDescending(x => x.NumberOfAgentsOnSkillGroup);
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
						return true;
					}
				}
			}
			return false;
		}
	}
}