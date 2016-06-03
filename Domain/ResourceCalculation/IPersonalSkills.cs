using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IPersonalSkills
	{
		IEnumerable<IPersonSkill> PersonSkills(IPersonPeriod period);
	}
}