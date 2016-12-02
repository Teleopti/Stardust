﻿using System.Collections.Generic;
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
			var groupedSkillGroups = skillGroups.GroupBy(x => x, (group, groups) => groups, new SkillGroupComparerForIslands());
			reduceSkillGroups(groupedSkillGroups);
			return groupedSkillGroups.Select(skillGroupInIsland => new Island(skillGroupInIsland)).ToList();
		}

		private void reduceSkillGroups(IEnumerable<IEnumerable<SkillGroup>> groupedSkillGroups)
		{
			var noAgentsKnowingSkill = numberOfAgentsKnowingSkill(groupedSkillGroups);

			foreach (var skillGroupsOnIsland in groupedSkillGroups)
			{
				var numberOfAgentsInIsland = skillGroupsOnIsland.Sum(x => x.Agents.Count());
				if (numberOfAgentsInIsland < _reduceIslandsLimits.MinimumNumberOfAgentsInIsland)
					continue;
				foreach (var skillGroup in skillGroupsOnIsland)
				{
					foreach (var skillGroupSkill in skillGroup.Skills.Reverse())
					{
						var agentsInSkillGroup = skillGroup.Agents.Count();
						if (agentsInSkillGroup *_reduceIslandsLimits.MinimumFactorOfAgentsInOtherSkillGroup >= noAgentsKnowingSkill[skillGroupSkill])
							continue;

						skillGroup.Skills.Remove(skillGroupSkill); //TODO! : förlorar förmodligen lite agenter här - kolla
						noAgentsKnowingSkill[skillGroupSkill] -= agentsInSkillGroup;
					}
				}
			}
		}

		private static Dictionary<ISkill, int> numberOfAgentsKnowingSkill(IEnumerable<IEnumerable<SkillGroup>> groupedSkillGroups)
		{
			var numberOfAgentsKnowingSkill = new Dictionary<ISkill, int>();
			foreach (var skillGroupsOnIsland in groupedSkillGroups)
			{
				foreach (var skillGroup in skillGroupsOnIsland)
				{
					foreach (var skill in skillGroup.Skills)
					{
						if (!numberOfAgentsKnowingSkill.ContainsKey(skill))
						{
							numberOfAgentsKnowingSkill[skill] = 0;
						}
						numberOfAgentsKnowingSkill[skill] += skillGroup.Agents.Count();
					}
				}
			}
			return numberOfAgentsKnowingSkill;
		}
	}
}