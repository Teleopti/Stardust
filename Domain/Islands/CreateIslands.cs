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
		private readonly NumberOfAgentsKnowingSkill _numberOfAgentsKnowingSkill;

		public CreateIslands(CreateSkillGroups createSkillGroups,
												IPeopleInOrganization peopleInOrganization,
												ReduceIslandsLimits reduceIslandsLimits,
												NumberOfAgentsKnowingSkill numberOfAgentsKnowingSkill)
		{
			_createSkillGroups = createSkillGroups;
			_peopleInOrganization = peopleInOrganization;
			_reduceIslandsLimits = reduceIslandsLimits;
			_numberOfAgentsKnowingSkill = numberOfAgentsKnowingSkill;
		}

		public IEnumerable<IIsland> Create(DateOnlyPeriod period)
		{
			var skillGroups = _createSkillGroups.Create(_peopleInOrganization.Agents(period), period.StartDate);

			var noAgentsKnowingSkill = _numberOfAgentsKnowingSkill.Execute(skillGroups);
			IEnumerable<List<SkillGroup>> skillGroupsInIsland = null;
			var reducing = true;
			while (reducing)
			{
				var skillGroupsInSeperateIslands = skillGroups.Select(skillGroup => new List<SkillGroup> { skillGroup }).ToList();
				moveSkillGroupToCorrectIsland(skillGroupsInSeperateIslands);
				skillGroupsInIsland = skillGroupsInSeperateIslands.Where(x => x.Count > 0);

				reducing = reduceSkillGroups(skillGroupsInIsland, noAgentsKnowingSkill);
			}

			return skillGroupsInIsland.Select(skillGroupInIsland => new Island(skillGroupInIsland, noAgentsKnowingSkill)).ToList();
		}

		private static void moveSkillGroupToCorrectIsland(IList<List<SkillGroup>> skillGroupInIslands)
		{
			foreach (var skillGroupInIsland in skillGroupInIslands)
			{
				foreach (var skillGroup in skillGroupInIsland.ToList())
				{
					var allOtherIslands = skillGroupInIslands.Except(new[] { skillGroupInIsland });
					foreach (var otherIsland in allOtherIslands)
					{
						foreach (var otherSkillGroup in otherIsland.ToArray())
						{
							if (otherSkillGroup.Skills.Intersect(skillGroup.Skills).Any())
							{
								otherIsland.Add(skillGroup);
								skillGroupInIsland.Remove(skillGroup);
							}
						}
					}
				}
			}
		}

		private bool reduceSkillGroups(IEnumerable<IEnumerable<SkillGroup>> groupedSkillGroups, IDictionary<ISkill, int> noAgentsKnowingSkill)
		{
			foreach (var skillGroupsOnIsland in groupedSkillGroups)
			{
				var numberOfAgentsInIsland = skillGroupsOnIsland.Sum(x => x.Agents.Count());
				if (numberOfAgentsInIsland < _reduceIslandsLimits.MinimumNumberOfAgentsInIsland)
					continue;
				var skillGroupsOnIslandAndNumberOfAgents = skillGroupsOnIsland.Select(x => new {x.Skills, NumberOfAgents = x.Agents.Count()}).OrderByDescending(x => x.NumberOfAgents);
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