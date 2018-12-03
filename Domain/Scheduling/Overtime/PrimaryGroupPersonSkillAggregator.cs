using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public class PrimaryGroupPersonSkillAggregator : IGroupPersonSkillAggregator
	{
		private readonly PersonalSkillsProvider _personalSkillsProvider;

		public PrimaryGroupPersonSkillAggregator(PersonalSkillsProvider personalSkillsProvider)
		{
			_personalSkillsProvider = personalSkillsProvider;
		}

		public IEnumerable<ISkill> AggregatedSkills(IEnumerable<IPerson> groupMembers, DateOnlyPeriod dateOnlyPeriod)
		{
			return groupMembers.SelectMany(p => p.PersonPeriods(dateOnlyPeriod))
				.SelectMany(pp => _personalSkillsProvider.PersonSkills(pp)).Select(ps => ps.Skill).Distinct().ToArray();
		}
	}
}