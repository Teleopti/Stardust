using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class ReduceSkillGroups
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
				var skillGroupsOnIslandAndNumberOfAgents = skillGroupsOnIsland.Select(x => new { x.Skills, NumberOfAgents = x.Agents.Count() }).OrderByDescending(x => x.NumberOfAgents);
				foreach (var skillGroupAndNumberOfAgents in skillGroupsOnIslandAndNumberOfAgents)
				{
					foreach (var skillGroupSkill in skillGroupAndNumberOfAgents.Skills.ToArray())
					{
						if (skillGroupAndNumberOfAgents.NumberOfAgents * _reduceIslandsLimits.MinimumFactorOfAgentsInOtherSkillGroup >= noAgentsKnowingSkill[skillGroupSkill])
							continue;

						if (skillGroupAndNumberOfAgents.Skills.Count(x => x.Activity == null || x.Activity.Equals(skillGroupSkill.Activity)) < 2)
							continue;

						skillGroupAndNumberOfAgents.Skills.Remove(skillGroupSkill);
						noAgentsKnowingSkill[skillGroupSkill] -= skillGroupAndNumberOfAgents.NumberOfAgents;
						return true;
					}
				}
			}
			return false;
		}
	}
}