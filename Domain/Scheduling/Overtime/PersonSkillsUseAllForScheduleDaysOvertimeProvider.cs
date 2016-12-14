using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	[RemoveMeWithToggle("When removing toggle, this type shouldn't impl any interface")]
	public class PersonSkillsUseAllForScheduleDaysOvertimeProvider : IPersonSkillsForScheduleDaysOvertimeProvider
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