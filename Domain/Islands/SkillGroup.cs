using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class SkillGroup
	{
		public SkillGroup(ICollection<ISkill> skills, ICollection<IPerson> agents)
		{
			Skills = skills;
			Agents = agents;
		}

		public ICollection<IPerson> Agents { get; }
		public ICollection<ISkill> Skills { get; }

		public bool HasSameSkillsAs(SkillGroup otherSkillGroup)
		{
			var theseSkills = new HashSet<ISkill>(Skills);
			var otherSkills = new HashSet<ISkill>(otherSkillGroup.Skills);
			return theseSkills.SetEquals(otherSkills);
		}
	}
}