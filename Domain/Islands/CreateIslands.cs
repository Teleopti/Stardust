using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class CreateIslands : ICreateIslands
	{
		private readonly CreateSkillGroups _createSkillGroups;
		private readonly NumberOfAgentsKnowingSkill _numberOfAgentsKnowingSkill;

		public CreateIslands(CreateSkillGroups createSkillGroups, NumberOfAgentsKnowingSkill numberOfAgentsKnowingSkill)
		{
			_createSkillGroups = createSkillGroups;
			_numberOfAgentsKnowingSkill = numberOfAgentsKnowingSkill;
		}

		public IEnumerable<IIsland> Create(IReduceSkillGroups reduceSkillGroups, IEnumerable<IPerson> peopleInOrganization, DateOnlyPeriod period)
		{
			var allSkillGroups = new List<SkillGroup>(_createSkillGroups.Create(peopleInOrganization, period.StartDate));
			var noAgentsKnowingSkill = _numberOfAgentsKnowingSkill.Execute(allSkillGroups);
			while (true)
			{
				var skillGroupsInIslands = allSkillGroups.Select(skillGroup => new List<SkillGroup> { skillGroup }).ToList();
				while (moveSkillGroupToCorrectIsland(allSkillGroups, skillGroupsInIslands))
				{
					removeEmptyIslands(skillGroupsInIslands);
				}

				if (!reduceSkillGroups.Execute(skillGroupsInIslands, noAgentsKnowingSkill))
				{
					return skillGroupsInIslands.Select(skillGroupInIsland => new Island(skillGroupInIsland, noAgentsKnowingSkill)).ToArray();
				}
			}
		}

		private static void removeEmptyIslands(ICollection<List<SkillGroup>> skillGroupsInIslands)
		{
			skillGroupsInIslands.Where(x => x.Count == 0).ToArray().ForEach(x => skillGroupsInIslands.Remove(x));
		}

		private static bool moveSkillGroupToCorrectIsland(ICollection<SkillGroup> allSkillGroups, IEnumerable<ICollection<SkillGroup>> skillGroupsInIslands)
		{
			var touchedIslands = new HashSet<ICollection<SkillGroup>>();

			foreach (var skillGroupInIsland in skillGroupsInIslands)
			{
				foreach (var skillGroup in skillGroupInIsland.ToArray())
				{
					var allOtherIslands = skillGroupsInIslands.Except(new[] { skillGroupInIsland });
					foreach (var otherIsland in allOtherIslands)
					{
						foreach (var otherSkillGroup in otherIsland.ToArray())
						{
							if(touchedIslands.Contains(skillGroupInIsland)) //move this if "earlier"
								continue;
							
							if (otherSkillGroup.HasAnySkillSameAs(skillGroup)) 
							{
								if (skillGroup.HasSameSkillsAs(otherSkillGroup))
								{
									otherSkillGroup.AddAgentsFrom(skillGroup);
									allSkillGroups.Remove(skillGroup);
								}
								else
								{
									otherIsland.Add(skillGroup);
								}
								touchedIslands.Add(skillGroupInIsland);
								touchedIslands.Add(otherIsland);
								skillGroupInIsland.Remove(skillGroup);
							}
						}
					}
				}
			}
			return touchedIslands.Any();
		}
	}
}