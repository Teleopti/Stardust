using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class CreateIslands : ICreateIslands
	{
		private readonly CreateSkillGroups _createSkillGroups;
		private readonly NumberOfAgentsKnowingSkill _numberOfAgentsKnowingSkill;
		private readonly MoveSkillGroupToCorrectIsland _moveSkillGroupToCorrectIsland;

		public CreateIslands(CreateSkillGroups createSkillGroups, 
								NumberOfAgentsKnowingSkill numberOfAgentsKnowingSkill,
								MoveSkillGroupToCorrectIsland moveSkillGroupToCorrectIsland)
		{
			_createSkillGroups = createSkillGroups;
			_numberOfAgentsKnowingSkill = numberOfAgentsKnowingSkill;
			_moveSkillGroupToCorrectIsland = moveSkillGroupToCorrectIsland;
		}

		[RemoveMeWithToggle("When toggle is gone, pass in reduceskillgroups as ctor arg", Toggles.ResourcePlanner_SplitBigIslands_42049)]
		public IEnumerable<IIsland> Create(IReduceSkillGroups reduceSkillGroups, IEnumerable<IPerson> peopleInOrganization, DateOnlyPeriod period)
		{
			var allSkillGroups = new List<SkillGroup>(_createSkillGroups.Create(peopleInOrganization, period.StartDate));
			var noAgentsKnowingSkill = _numberOfAgentsKnowingSkill.Execute(allSkillGroups);
			while (true)
			{
				var skillGroupsInIslands = allSkillGroups.Select(skillGroup => new List<SkillGroup> { skillGroup }).ToList();
				while (_moveSkillGroupToCorrectIsland.Execute(allSkillGroups, skillGroupsInIslands))
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
	}
}