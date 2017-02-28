﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface ISkillCombinationResourceRepository
	{
		void PersistSkillCombinationResource(DateTime dataLoaded, IEnumerable<SkillCombinationResource> skillCombinationResources);
		IEnumerable<SkillCombinationResource> LoadSkillCombinationResources(DateTimePeriod period);
		void PersistChanges(IEnumerable<SkillCombinationResource> deltas);
		DateTime GetLastCalculatedTime();
	}
}