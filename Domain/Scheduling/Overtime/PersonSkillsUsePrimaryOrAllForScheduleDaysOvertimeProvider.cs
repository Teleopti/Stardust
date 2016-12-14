using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public class PersonSkillsUsePrimaryOrAllForScheduleDaysOvertimeProvider : IPersonSkillsForScheduleDaysOvertimeProvider
	{
		private readonly PersonSkillsUseAllForScheduleDaysOvertimeProvider _personSkillsUseAllForScheduleDaysOvertimeProvider;
		private readonly PersonalSkillsProvider _personalSkillsProvider;

		public PersonSkillsUsePrimaryOrAllForScheduleDaysOvertimeProvider(PersonSkillsUseAllForScheduleDaysOvertimeProvider personSkillsUseAllForScheduleDaysOvertimeProvider, PersonalSkillsProvider personalSkillsProvider)
		{
			_personSkillsUseAllForScheduleDaysOvertimeProvider = personSkillsUseAllForScheduleDaysOvertimeProvider;
			_personalSkillsProvider = personalSkillsProvider;
		}

		public IEnumerable<ISkill> Execute(IOvertimePreferences overtimePreferences, IPersonPeriod personPeriod)
		{
			return overtimePreferences.UseSkills == UseSkills.Primary ? 
				new HashSet<ISkill>(_personalSkillsProvider.PersonSkills(personPeriod).Select(x => x.Skill)).Where(x => x.Activity.Equals(overtimePreferences.SkillActivity)) : 
				_personSkillsUseAllForScheduleDaysOvertimeProvider.Execute(overtimePreferences, personPeriod);
		}
	}
}