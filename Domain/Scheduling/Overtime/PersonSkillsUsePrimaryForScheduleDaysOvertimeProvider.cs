using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public class PersonSkillsUsePrimaryForScheduleDaysOvertimeProvider : IPersonSkillsForScheduleDaysOvertimeProvider
	{
		private readonly PersonalSkillsProvider _personalSkillsProvider;

		public PersonSkillsUsePrimaryForScheduleDaysOvertimeProvider(PersonalSkillsProvider personalSkillsProvider)
		{
			_personalSkillsProvider = personalSkillsProvider;
		}

		public IEnumerable<ISkill> Execute(IPersonPeriod personPeriod, IActivity activity)
		{
			return new HashSet<ISkill>(_personalSkillsProvider.PersonSkills(personPeriod).Select(x => x.Skill)).Where(x => x.Activity.Equals(activity));
		}
	}
}