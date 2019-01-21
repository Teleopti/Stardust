using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IGroupPersonSkillAggregator
	{
		IEnumerable<ISkill> AggregatedSkills(IEnumerable<IPerson> groupMembers, DateOnlyPeriod dateOnlyPeriod);
	}

	public class GroupPersonSkillAggregator : IGroupPersonSkillAggregator
	{
		private readonly PersonalSkillsProvider _personalSkillsProvider;

		public GroupPersonSkillAggregator(PersonalSkillsProvider personalSkillsProvider)
		{
			_personalSkillsProvider = personalSkillsProvider;
		}

		public IEnumerable<ISkill> AggregatedSkills(IEnumerable<IPerson> groupMembers, DateOnlyPeriod dateOnlyPeriod)
		{
			return groupMembers.SelectMany(person =>
				person.PersonPeriods(dateOnlyPeriod)
					.SelectMany(p => _personalSkillsProvider.PersonSkillsBasedOnPrimarySkill(p).Select(pp => pp.Skill)))
					.Distinct().ToArray();
		}
	}
}