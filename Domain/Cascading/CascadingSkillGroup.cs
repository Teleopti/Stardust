using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingSkillGroup
	{
		public CascadingSkillGroup(IEnumerable<ISkill> primarySkills, IEnumerable<CascadingSkillGroupItem> cascadingSkillGroupItems, double resources)
		{
			PrimarySkills = primarySkills;
			CascadingSkillGroupItems = cascadingSkillGroupItems;
			Resources = resources;
		}

		public IEnumerable<ISkill> PrimarySkills { get; }
		public IEnumerable<CascadingSkillGroupItem> CascadingSkillGroupItems { get; }
		public double Resources { get; }
	}
}