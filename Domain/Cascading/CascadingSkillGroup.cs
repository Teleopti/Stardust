using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingSkillGroup
	{
		public CascadingSkillGroup(ISkill primarySkill, IEnumerable<CascadingSkillGroupItem> cascadingSkills, double resources)
		{
			PrimarySkill = primarySkill;
			CascadingSkills = cascadingSkills;
			Resources = resources;
		}

		public ISkill PrimarySkill { get; }
		public IEnumerable<CascadingSkillGroupItem> CascadingSkills { get; }
		public double Resources { get; }
	}
}