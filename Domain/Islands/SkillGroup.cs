using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class SkillGroup
	{
		private readonly ICollection<IPerson> _agents;
		private readonly ICollection<ISkill> _skills;

		public SkillGroup(ICollection<ISkill> skills, ICollection<IPerson> agents)
		{
			_skills = skills;
			_agents = agents;
		}

		public IEnumerable<IPerson> Agents => _agents;
		public IEnumerable<ISkill> Skills => _skills;

		public bool HasSameSkillsAs(SkillGroup otherSkillGroup)
		{
			var theseSkills = new HashSet<ISkill>(Skills);
			var otherSkills = new HashSet<ISkill>(otherSkillGroup.Skills);
			return theseSkills.SetEquals(otherSkills);
		}

		public void AddAgentsFrom(SkillGroup skillGroup)
		{
			skillGroup.Agents.ForEach(x => _agents.Add(x));
		}

		public void RemoveSkill(ISkill skill)
		{
			_skills.Remove(skill);
		}
	}
}