using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public static class SchedulingResultStateHolderExtensions
	{
		public static int SetSkills(this ISchedulingResultStateHolder stateHolder,
			ILoaderDeciderResult deciderResult, IEnumerable<ISkill> allSkills)
		{
			var skills = new HashSet<ISkill>(allSkills);
			var ret = deciderResult.FilterSkills(allSkills, x => skills.Remove(x), x => skills.Add(x));
			stateHolder.Skills = skills;
			return ret;
		}
	}
}