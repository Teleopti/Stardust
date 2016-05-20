using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingSkillGroup
	{
		public CascadingSkillGroup(ISkill primarySkill, IEnumerable<ISkill> cascadingSkills, double resources)
		{
			PrimarySkill = primarySkill;
			CascadingSkills = cascadingSkills;
			Resources = resources;
		}

		public ISkill PrimarySkill { get; }
		public IEnumerable<ISkill> CascadingSkills { get; }
		public double Resources { get; }
	}
}