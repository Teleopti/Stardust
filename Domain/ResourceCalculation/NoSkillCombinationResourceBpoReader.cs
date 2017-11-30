using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class NoSkillCombinationResourceReader : ISkillCombinationResourceReader
	{
		public IEnumerable<SkillCombinationResource> Execute(DateTimePeriod period)
		{
			yield break;
		}
	}
}