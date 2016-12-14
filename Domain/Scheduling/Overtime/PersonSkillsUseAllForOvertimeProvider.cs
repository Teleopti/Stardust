using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public class PersonSkillsUseAllForOvertimeProvider : IPersonSkillsForOvertimeProvider
	{
		public IEnumerable<ISkill> Execute(IPersonPeriod personPeriod, IActivity activity)
		{
			var ret = new List<ISkill>();
			foreach (var personSkill in personPeriod.PersonSkillCollection)
			{
				var skill = personSkill.Skill;
				if (!skill.Activity.Equals(activity))
					continue;
				if (!ret.Contains(skill))
					ret.Add(skill);
			}
			return ret;
		}
	}
}