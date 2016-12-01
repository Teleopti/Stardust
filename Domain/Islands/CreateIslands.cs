using System.Collections.Generic;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_SplitBigIslands_42049)]
	public interface ICreateIslands
	{
		IEnumerable<Island> Create(DateOnlyPeriod period);
	}

	public class CreateIslands : ICreateIslands
	{
		private readonly VirtualSkillGroupsCreator _virtualSkillGroupsCreator;
		private readonly SkillGroupIslandsAnalyzer _skillGroupIslandsAnalyzer;
		private readonly IPeopleInOrganization _peopleInOrganization;

		public CreateIslands(VirtualSkillGroupsCreator virtualSkillGroupsCreator,
												SkillGroupIslandsAnalyzer skillGroupIslandsAnalyzer,
												IPeopleInOrganization peopleInOrganization)
		{
			_virtualSkillGroupsCreator = virtualSkillGroupsCreator;
			_skillGroupIslandsAnalyzer = skillGroupIslandsAnalyzer;
			_peopleInOrganization = peopleInOrganization;
		}

		public IEnumerable<Island> Create(DateOnlyPeriod period)
		{
			var skillGroupsCreatorResult = _virtualSkillGroupsCreator.GroupOnDate(period.StartDate, _peopleInOrganization.Agents(period));
			return _skillGroupIslandsAnalyzer.FindIslands(skillGroupsCreatorResult);
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

		public IEnumerable<Island> Create(DateOnlyPeriod period)
		{
			var skillGroupsCreatorResult = _virtualSkillGroupsCreator.GroupOnDate(period.StartDate, _peopleInOrganization.Agents(period));
			return _skillGroupIslandsAnalyzer.FindIslands(skillGroupsCreatorResult);
		}
	}
}