using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands.Legacy
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_SplitBigIslands_42049)]
	public class CreateIslandsOld : ICreateIslands
	{
		private readonly IVirtualSkillGroupsCreator _virtualSkillGroupsCreator;
		private readonly SkillGroupIslandsAnalyzer _skillGroupIslandsAnalyzer;

		public CreateIslandsOld(IVirtualSkillGroupsCreator virtualSkillGroupsCreator,
												SkillGroupIslandsAnalyzer skillGroupIslandsAnalyzer)
		{
			_virtualSkillGroupsCreator = virtualSkillGroupsCreator;
			_skillGroupIslandsAnalyzer = skillGroupIslandsAnalyzer;
		}

		public IEnumerable<IIsland> Create(IReduceSkillGroups reduceSkillGroups, IEnumerable<IPerson> peopleInOrganization, DateOnlyPeriod period)
		{
			var skillGroupsCreatorResult = _virtualSkillGroupsCreator.GroupOnDate(period.StartDate, peopleInOrganization);
			return _skillGroupIslandsAnalyzer.FindIslands(skillGroupsCreatorResult);
		}
	}
}