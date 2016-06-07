using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class PersonalSkillsBasedOnPrimarySkillModeAlwaysUseAllPersonalSkills : IPersonalSkillsBasedOnPrimarySkillMode
	{
		public IEnumerable<IPersonSkill> PersonalSkills(IPersonPeriod personPeriod)
		{
			return new PersonalSkills().PersonSkills(personPeriod);
		}
	}
}