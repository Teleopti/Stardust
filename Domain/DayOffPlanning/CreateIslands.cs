using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
	public class CreateIslands
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
}