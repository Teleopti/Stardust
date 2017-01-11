using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface ISkillCombinationResourceRepository
	{
		void PersistSkillCombinationResource(DateTime dataLoaded, IEnumerable<SkillCombinationResource> skillCombinationResources);
		IEnumerable<SkillCombinationResource> LoadSkillCombinationResources(DateTimePeriod period);
		void PersistChange(SkillCombinationResource skillCombinationResource);
		DateTime GetLastCalculatedTime();
	}
}