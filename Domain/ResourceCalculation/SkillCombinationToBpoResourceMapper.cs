using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SkillCombinationToBpoResourceMapper
	{
		public IEnumerable<ExternalStaff> Execute(IEnumerable<SkillCombinationResource> skillCombinationResources, IEnumerable<ISkill> skills)
		{
			return skillCombinationResources.Select(skillCombinationResource => new ExternalStaff(skillCombinationResource.Resource,
				skills.Where(x => skillCombinationResource.SkillCombination.Contains(x.Id.Value)),
				skillCombinationResource.Period()));
		}
	}
}