using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Islands.Legacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class CreateIslands : ICreateIslands
	{
		private readonly CreateSkillGroups _createSkillGroups;
		private readonly IPeopleInOrganization _peopleInOrganization;

		public CreateIslands(CreateSkillGroups createSkillGroups,
												IPeopleInOrganization peopleInOrganization,
												ReduceIslandsLimits reduceIslandsLimits)
		{
			_createSkillGroups = createSkillGroups;
			_peopleInOrganization = peopleInOrganization;
		}

		public IEnumerable<IIsland> Create(DateOnlyPeriod period)
		{
			var skillGroups = _createSkillGroups.Create(_peopleInOrganization.Agents(period), period.StartDate);
			var groupedSkillGroups = skillGroups.GroupBy(x => x, (group, groups) => groups, new SkillGroupComparerForIslands());
			reduceSkillGroups(groupedSkillGroups);
			

			var ret = new List<IIsland>();
			foreach (var skillGroupInIsland in groupedSkillGroups)
			{
				ret.Add(new Island(skillGroupInIsland));
			}
			return ret;
		}

		private void reduceSkillGroups(IEnumerable<IEnumerable<SkillGroup>> groupedSkillGroups)
		{
			foreach (var groupedSkillGroup in groupedSkillGroups)
			{
				foreach (var skillGroup in groupedSkillGroup)
				{
					foreach (var skillGroupSkill in skillGroup.Skills.Reverse())
					{
						foreach (var otherSkillGroup in groupedSkillGroup)
						{
							if (otherSkillGroup == skillGroup) continue;
							if (!otherSkillGroup.Skills.Contains(skillGroupSkill)) continue;
							if (skillGroup.Skills.Count > 1 && skillGroup.Agents.Count() * 4 < otherSkillGroup.Agents.Count())
							{
								skillGroup.Skills.Remove(skillGroupSkill);
							}
						}
					}
				}
			}	
		}
	}


	[RemoveMeWithToggle(Toggles.ResourcePlanner_SplitBigIslands_42049)]
	public class CreateIslandsOld : ICreateIslands
	{
		private readonly VirtualSkillGroupsCreator _virtualSkillGroupsCreator;
		private readonly SkillGroupIslandsAnalyzer _skillGroupIslandsAnalyzer;
		private readonly IPeopleInOrganization _peopleInOrganization;

		public CreateIslandsOld(VirtualSkillGroupsCreator virtualSkillGroupsCreator,
												SkillGroupIslandsAnalyzer skillGroupIslandsAnalyzer,
												IPeopleInOrganization peopleInOrganization)
		{
			_virtualSkillGroupsCreator = virtualSkillGroupsCreator;
			_skillGroupIslandsAnalyzer = skillGroupIslandsAnalyzer;
			_peopleInOrganization = peopleInOrganization;
		}

		public IEnumerable<IIsland> Create(DateOnlyPeriod period)
		{
			var skillGroupsCreatorResult = _virtualSkillGroupsCreator.GroupOnDate(period.StartDate, _peopleInOrganization.Agents(period));
			return _skillGroupIslandsAnalyzer.FindIslands(skillGroupsCreatorResult);
		}
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_SplitBigIslands_42049)]
	public interface IIsland
	{
		IEnumerable<IPerson> AgentsInIsland();
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_SplitBigIslands_42049)]
	public interface ICreateIslands
	{
		IEnumerable<IIsland> Create(DateOnlyPeriod period);
	}
}