using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class PrimaryPersonSkillFilter : IPrimaryPersonSkillFilter
	{
		public IList<IPersonSkill> Filter(IEnumerable<IPersonSkill> personSkills)
		{
			var cascadingPersonSkills= personSkills.Where(a => a.Skill.IsCascading());
			if (!cascadingPersonSkills.Any())
			{
				return personSkills.ToList();
			}
			return cascadingPersonSkills.Where(a => a.Skill.CascadingIndex == 1).ToList();
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