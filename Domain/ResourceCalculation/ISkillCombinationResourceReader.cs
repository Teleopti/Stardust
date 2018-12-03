using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface ISkillCombinationResourceReader
	{
		IEnumerable<SkillCombinationResource> Execute(DateTimePeriod period);
	}
}