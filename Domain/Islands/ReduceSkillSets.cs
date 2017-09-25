using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class ReduceSkillSets : IReduceSkillSets
	{
		private readonly ReduceIslandsLimits _reduceIslandsLimits;

		public ReduceSkillSets(ReduceIslandsLimits reduceIslandsLimits)
		{
			_reduceIslandsLimits = reduceIslandsLimits;
		}

		public bool Execute(IEnumerable<IEnumerable<SkillSet>> groupedSkillSets, IDictionary<ISkill, int> noAgentsKnowingSkill)
		{
			var success = false;
		
			foreach (var island in groupedSkillSets)
			{
				var removed = 0;
				var numberOfAgentsInIsland = island.Sum(x => x.Agents.Count());
				if (numberOfAgentsInIsland < _reduceIslandsLimits.MinimumNumberOfAgentsInIsland)
					continue;
				var skillSetsOnIslandAndNumberOfAgents = island.Select(x => new { SkillSet = x, NumberOfAgentsOnSkillSet = x.Agents.Count() }).OrderByDescending(x => x.NumberOfAgentsOnSkillSet);
				foreach (var skillSetAndNumberOfAgents in skillSetsOnIslandAndNumberOfAgents)
				{
					foreach (var skillSetSkill in skillSetAndNumberOfAgents.SkillSet.Skills.ToArray())
					{
						if (skillSetAndNumberOfAgents.NumberOfAgentsOnSkillSet * _reduceIslandsLimits.MinimumFactorOfAgentsInOtherSkillSet(numberOfAgentsInIsland) >= noAgentsKnowingSkill[skillSetSkill])
							continue;

						if (skillSetAndNumberOfAgents.SkillSet.Skills.Count(x => x.Activity == null || x.Activity.Equals(skillSetSkill.Activity)) < 2)
							continue;

						skillSetAndNumberOfAgents.SkillSet.RemoveSkill(skillSetSkill);
						noAgentsKnowingSkill[skillSetSkill] -= skillSetAndNumberOfAgents.NumberOfAgentsOnSkillSet;

						removed += skillSetAndNumberOfAgents.NumberOfAgentsOnSkillSet;
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