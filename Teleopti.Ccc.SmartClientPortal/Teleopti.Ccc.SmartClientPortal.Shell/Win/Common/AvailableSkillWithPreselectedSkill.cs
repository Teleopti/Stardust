using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{
	public class AvailableSkillWithPreselectedSkill
	{
		public AvailableSkillWithPreselectedSkill(ISkill preselectedSkill, IEnumerable<ISkill> availableSkills)
		{
			AvailableSkills = availableSkills;
			PreselectedSkill = preselectedSkill;
		}

		public ISkill PreselectedSkill { get; private set; }
		public IEnumerable<ISkill> AvailableSkills { get; private set; }
	}
}