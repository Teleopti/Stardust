using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class CompareProjection
	{
		public IEnumerable<SkillCombinationResource> Compare(IScheduleDay before, IScheduleDay after)
		{
			return new List<SkillCombinationResource>();
		}
	}
}
