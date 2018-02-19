﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface ISkillCombinationResourceRepository
	{
		void PersistSkillCombinationResource(DateTime dataLoaded, IEnumerable<SkillCombinationResource> skillCombinationResources);
		IEnumerable<SkillCombinationResource> LoadSkillCombinationResources(DateTimePeriod period, bool useBpoExchange = true);
		void PersistChanges(IEnumerable<SkillCombinationResource> deltas);
		DateTime GetLastCalculatedTime();
		void PersistSkillCombinationResourceBpo(List<ImportSkillCombinationResourceBpo> combinationResources);
		Dictionary<Guid, string> LoadSourceBpo(SqlConnection connection);
		IEnumerable<SkillCombinationResourceForBpo> BpoResourcesForSkill(Guid skillId, DateOnlyPeriod period);
		IEnumerable<ScheduledHeads> ScheduledHeadsForSkill(Guid skillId, DateOnlyPeriod period);
	}
}