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
		private readonly NumberOfAgentsKnowingSkill _numberOfAgentsKnowingSkill;
		private readonly ReduceSkillGroups _reduceSkillGroups;

		public CreateIslands(CreateSkillGroups createSkillGroups,
												IPeopleInOrganization peopleInOrganization,
												NumberOfAgentsKnowingSkill numberOfAgentsKnowingSkill,
												ReduceSkillGroups reduceSkillGroups)
		{
			_createSkillGroups = createSkillGroups;
			_peopleInOrganization = peopleInOrganization;
			_numberOfAgentsKnowingSkill = numberOfAgentsKnowingSkill;
			_reduceSkillGroups = reduceSkillGroups;
		}

		public IEnumerable<IIsland> Create(DateOnlyPeriod period)
		{
			var skillGroups = _createSkillGroups.Create(_peopleInOrganization.Agents(period), period.StartDate);

			var noAgentsKnowingSkill = _numberOfAgentsKnowingSkill.Execute(skillGroups);
			IEnumerable<List<SkillGroup>> skillGroupsInIsland = null;
			var keepOnReducing = true;
			while (keepOnReducing)
			{
				var skillGroupsInSeperateIslands = skillGroups.Select(skillGroup => new List<SkillGroup> { skillGroup }).ToList();
				moveSkillGroupToCorrectIsland(skillGroupsInSeperateIslands);
				skillGroupsInIsland = skillGroupsInSeperateIslands.Where(x => x.Count > 0);

				keepOnReducing = _reduceSkillGroups.Execute(skillGroupsInIsland, noAgentsKnowingSkill);
			}

			return skillGroupsInIsland.Select(skillGroupInIsland => new Island(skillGroupInIsland, noAgentsKnowingSkill)).ToList();
		}

		private static void moveSkillGroupToCorrectIsland(IEnumerable<ICollection<SkillGroup>> skillGroupInIslands)
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
	}
}