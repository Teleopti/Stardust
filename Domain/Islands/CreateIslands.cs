using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class CreateIslands : ICreateIslands
	{
		private readonly CreateSkillGroups _createSkillGroups;
		private readonly IPeopleInOrganization _peopleInOrganization;
		private readonly ReduceIslandsLimits _reduceIslandsLimits;

		public CreateIslands(CreateSkillGroups createSkillGroups,
												IPeopleInOrganization peopleInOrganization,
												ReduceIslandsLimits reduceIslandsLimits)
		{
			_createSkillGroups = createSkillGroups;
			_peopleInOrganization = peopleInOrganization;
			_reduceIslandsLimits = reduceIslandsLimits;
		}

		public IEnumerable<IIsland> Create(DateOnlyPeriod period)
		{
			var skillGroups = _createSkillGroups.Create(_peopleInOrganization.Agents(period), period.StartDate);
			var noAgentsKnowingSkill = numberOfAgentsKnowingSkill(skillGroups);
			var groupedSkillGroups = skillGroups.GroupBy(x => x, (group, groups) => groups, new SkillGroupComparerForIslands());
			reduceSkillGroups(groupedSkillGroups, noAgentsKnowingSkill);
			return groupedSkillGroups.Select(skillGroupInIsland => new Island(skillGroupInIsland)).ToList();
		}

		private void reduceSkillGroups(IEnumerable<IEnumerable<SkillGroup>> groupedSkillGroups, IDictionary<ISkill, int> noAgentsKnowingSkill)
		{
			foreach (var skillGroupsOnIsland in groupedSkillGroups)
			{
				var numberOfAgentsInIsland = skillGroupsOnIsland.Sum(x => x.Agents.Count());
				if (numberOfAgentsInIsland < _reduceIslandsLimits.MinimumNumberOfAgentsInIsland)
					continue;
				foreach (var skillGroup in skillGroupsOnIsland)
				{
					var agentsInSkillGroup = skillGroup.Agents.Count();
					foreach (var skillGroupSkill in skillGroup.Skills.Reverse())
					{
						if (agentsInSkillGroup *_reduceIslandsLimits.MinimumFactorOfAgentsInOtherSkillGroup >= noAgentsKnowingSkill[skillGroupSkill])
							continue;

						if (skillGroup.Skills.Count(x => x.Activity == null || x.Activity.Equals(skillGroupSkill.Activity)) < 2)
							continue; 

						skillGroup.Skills.Remove(skillGroupSkill); //TODO! : förlorar förmodligen lite agenter här - kolla
						noAgentsKnowingSkill[skillGroupSkill] -= agentsInSkillGroup;
					}
				}
			}
		}

		private static IDictionary<ISkill, int> numberOfAgentsKnowingSkill(IEnumerable<SkillGroup> skillGroups)
		{
			var numberOfAgentsKnowingSkill = new Dictionary<ISkill, int>();
			foreach (var skillGroup in skillGroups)
			{
				var count = skillGroup.Agents.Count();
				foreach (var skill in skillGroup.Skills)
				{
					if (!numberOfAgentsKnowingSkill.ContainsKey(skill))
					{
						numberOfAgentsKnowingSkill[skill] = 0;
					}
					numberOfAgentsKnowingSkill[skill] += count;
				}
			}
			return numberOfAgentsKnowingSkill;
		}
	}
}