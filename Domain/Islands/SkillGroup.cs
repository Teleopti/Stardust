using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class SkillGroup
	{
		private readonly ISet<IPerson> _agents;
		private readonly ISet<ISkill> _skills;

		public SkillGroup(ISet<ISkill> skills, ISet<IPerson> agents)
		{
			_skills = skills;
			_agents = agents;
		}

		public IEnumerable<IPerson> Agents => _agents;
		public IEnumerable<ISkill> Skills => _skills;

		public bool HasSameSkillsAs(SkillGroup otherSkillGroup)
		{
			return _skills.SetEquals(otherSkillGroup._skills);
		}

		public void AddAgentsFrom(SkillGroup skillGroup)
		{
			skillGroup.Agents.ForEach(x => _agents.Add(x));
		}

		public void RemoveSkill(ISkill skill)
		{
			_skills.Remove(skill);
		}

		public bool HasAnySkillSameAs(SkillGroup skillGroup)
		{
			return _skills.Overlaps(skillGroup._skills);
		}
	}
}