using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class SkillSet
	{
		private readonly ISet<IPerson> _agents;
		private readonly ISet<ISkill> _skills;

		public SkillSet(ISet<ISkill> skills, ISet<IPerson> agents)
		{
			_skills = skills;
			_agents = agents;
		}

		public IEnumerable<IPerson> Agents => _agents;
		public IEnumerable<ISkill> Skills => _skills;

		public bool HasSameSkillsAs(SkillSet otherSkillSet)
		{
			return _skills.SetEquals(otherSkillSet._skills);
		}

		public void AddAgentsFrom(SkillSet skillSet)
		{
			skillSet.Agents.ForEach(x => _agents.Add(x));
		}

		public void RemoveSkill(ISkill skill)
		{
			_skills.Remove(skill);
		}

		public bool HasAnySkillSameAs(SkillSet skillSet)
		{
			return _skills.Overlaps(skillSet._skills);
		}
	}
}