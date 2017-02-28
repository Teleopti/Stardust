using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public class PersonSkillsUseAllForScheduleDaysOvertimeProvider
	{
		public IEnumerable<ISkill> Execute(IOvertimePreferences overtimePreferences, IPersonPeriod personPeriod)
		{
			var ret = new HashSet<ISkill>();
			foreach (var personSkill in personPeriod.PersonSkillCollection)
			{
				var skill = personSkill.Skill;
				if (!skill.Activity.Equals(overtimePreferences.SkillActivity))
					continue;
				ret.Add(skill);
			}
			return ret;
		}
	}
}