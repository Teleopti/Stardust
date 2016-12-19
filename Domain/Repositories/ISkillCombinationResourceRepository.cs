using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface ISkillCombinationResourceRepository
	{
		void PersistSkillCombination(IEnumerable<IEnumerable<Guid>> skillCombination);
		Guid? LoadSkillCombination(IEnumerable<Guid> skillCombination);
		void PersistSkillCombinationResource(IEnumerable<SkillCombinationResource> skillCombinationResources);
		IEnumerable<SkillCombinationResource> LoadSkillCombinationResources(DateTimePeriod period);
	}
}