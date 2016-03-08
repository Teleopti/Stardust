using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
	public class CreateIslands
	{
		private readonly VirtualSkillGroupsCreator _virtualSkillGroupsCreator;
		private readonly SkillGroupIslandsAnalyzer _skillGroupIslandsAnalyzer;

		public CreateIslands(VirtualSkillGroupsCreator virtualSkillGroupsCreator,
												SkillGroupIslandsAnalyzer skillGroupIslandsAnalyzer)
		{
			_virtualSkillGroupsCreator = virtualSkillGroupsCreator;
			_skillGroupIslandsAnalyzer = skillGroupIslandsAnalyzer;
		}

		public IEnumerable<Island> Create(DateOnlyPeriod period, IEnumerable<IPerson> agents)
		{
			var skillGroupsCreatorResult = _virtualSkillGroupsCreator.GroupOnDate(period.StartDate, agents);
			return _skillGroupIslandsAnalyzer.FindIslands(skillGroupsCreatorResult);
		}
	}
}