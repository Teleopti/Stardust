using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Forecasting;
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
			
			var skillGroupInIslands = skillGroups.Select(skillGroup => new List<SkillGroup> {skillGroup}).ToList();
			moveSkillGroupToCorrectIsland(skillGroupInIslands);
			var skillGroupInIslandsEmptyRemoved = skillGroupInIslands.Where(x => x.Count > 0);


			reduceSkillGroups(skillGroupInIslandsEmptyRemoved, noAgentsKnowingSkill);
			return skillGroupInIslandsEmptyRemoved.Select(skillGroupInIsland => new Island(skillGroupInIsland)).ToList();
		}

		private static void moveSkillGroupToCorrectIsland(IList<List<SkillGroup>> skillGroupInIslands)
		{
			foreach (var skillGroupInIsland in skillGroupInIslands)
			{
				foreach (var skillGroup in skillGroupInIsland.ToList())
				{
					var allOtherIslands = skillGroupInIslands.Except(new[] {  skillGroupInIsland });
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
					foreach (var skillGroupSkill in skillGroup.Skills.ToArray())
					{
						if (agentsInSkillGroup *_reduceIslandsLimits.MinimumFactorOfAgentsInOtherSkillGroup >= noAgentsKnowingSkill[skillGroupSkill])
							continue;

						if (skillGroup.Skills.Count(x => x.Activity == null || x.Activity.Equals(skillGroupSkill.Activity)) < 2)
							continue; 

						skillGroup.Skills.Remove(skillGroupSkill); 
						noAgentsKnowingSkill[skillGroupSkill] -= agentsInSkillGroup;
					}
				}
			}
		}
	}
}