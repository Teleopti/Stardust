using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IPersonalSkillsBasedOnPrimarySkillMode
	{
		IEnumerable<IPersonSkill> PersonalSkills(IPersonPeriod personPeriod);
	}
}