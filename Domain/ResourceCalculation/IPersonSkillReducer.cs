using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IPersonSkillReducer
	{
		IEnumerable<IPersonSkill> Reduce(IPersonPeriod personPeriod);
	}
}