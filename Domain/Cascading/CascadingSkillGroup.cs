using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingSkillGroup
	{
		public CascadingSkillGroup(ISkill primarySkill, IEnumerable<CascadingSkillGroupItem> cascadingSkillGroupItems, double resources)
		{
			PrimarySkill = primarySkill;
			CascadingSkillGroupItems = cascadingSkillGroupItems;
			Resources = resources;
		}

		public ISkill PrimarySkill { get; }
		public IEnumerable<CascadingSkillGroupItem> CascadingSkillGroupItems { get; }
		public double Resources { get; }
	}
}