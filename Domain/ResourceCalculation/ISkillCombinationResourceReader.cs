using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface ISkillCombinationResourceReader
	{
		IEnumerable<SkillCombinationResource> Execute(DateTimePeriod period);
	}
}