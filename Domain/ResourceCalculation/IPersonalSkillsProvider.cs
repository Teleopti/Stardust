using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IPersonalSkillsProvider
	{
		IEnumerable<IPersonSkill> PersonSkills(IPersonPeriod period);
		IEnumerable<IPersonSkill> PersonSkillsBasedOnPrimarySkill(IPersonPeriod period);
	}
}