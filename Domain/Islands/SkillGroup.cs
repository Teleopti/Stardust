using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class SkillGroup
	{
		public SkillGroup(IEnumerable<ISkill> skills, IEnumerable<IPerson> agents)
		{
			Skills = skills;
			Agents = agents;
		}

		public IEnumerable<IPerson> Agents { get; }
		public IEnumerable<ISkill> Skills { get; }
	}
}