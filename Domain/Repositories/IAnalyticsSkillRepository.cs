using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsSkillRepository
	{
		int? SkillSetId(IList<AnalyticsSkill> skills);
		IList<AnalyticsSkill> Skills(int businessUnitId);
	}
}
