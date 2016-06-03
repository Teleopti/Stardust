using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class CascadingPersonSkillProvider : PersonSkillProvider
	{
		protected override IEnumerable<IPersonSkill> PersonSkills(IPersonPeriod personPeriod)
		{
			return new CascadingPersonalSkills().PersonSkills(personPeriod);
		}
	}
}