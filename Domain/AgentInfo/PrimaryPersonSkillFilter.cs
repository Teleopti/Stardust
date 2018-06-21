using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	[DisabledBy(Toggles.OvertimeRequestUsePrimarySkillOption_75573)]
	public class PrimaryPersonSkillFilterToggle75573Off : IPrimaryPersonSkillFilter
	{
		private const int primarySkillCascadingIndex = 1;

		public IList<IPersonSkill> Filter(IEnumerable<IPersonSkill> personSkills, IPerson person)
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

	[EnabledBy(Toggles.OvertimeRequestUsePrimarySkillOption_75573)]
	public class PrimaryPersonSkillFilter : IPrimaryPersonSkillFilter
	{
		public IList<IPersonSkill> Filter(IEnumerable<IPersonSkill> personSkills, IPerson person)
		{
			if (!person.WorkflowControlSet.OvertimeRequestUsePrimarySkill)
			{
				return personSkills.ToList();
			}

			var minCascadingIndex =
				personSkills.Min(personSkill => personSkill.Skill.CascadingIndex);

			var primaryPersonSkills =
				personSkills.Where(a => a.Skill.IsCascading() && a.Skill.CascadingIndex == minCascadingIndex).ToList();
			if (!primaryPersonSkills.Any())
			{
				return personSkills.ToList();
			}
			return primaryPersonSkills;
		}
	}
}