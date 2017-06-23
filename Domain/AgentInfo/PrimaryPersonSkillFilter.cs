using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class PrimaryPersonSkillFilter : IPrimaryPersonSkillFilter
	{
		private const int primarySkillCascadingIndex = 1;

		public IList<IPersonSkill> Filter(IEnumerable<IPersonSkill> personSkills)
		{
			var primaryPersonSkills =
				personSkills.Where(a => a.Skill.IsCascading() && a.Skill.CascadingIndex == primarySkillCascadingIndex).ToList();
			if (!primaryPersonSkills.Any())
			{
				return personSkills.ToList();
			}
			return primaryPersonSkills;
		}
	}

	public class PrimaryPersonSkillFilterToggle44686Off : IPrimaryPersonSkillFilter
	{
		public IList<IPersonSkill> Filter(IEnumerable<IPersonSkill> personSkills)
		{
			return personSkills.ToList();
		}
	}
}