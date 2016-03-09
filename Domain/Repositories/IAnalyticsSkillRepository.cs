using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsSkillRepository
	{
		IList<AnalyticsSkillSet> SkillSets();
		int? SkillSetId(IList<AnalyticsSkill> skills);
		IList<AnalyticsSkill> Skills(int businessUnitId);
		int AddSkillSet(AnalyticsSkillSet analyticsSkillSet);
		void AddBridgeSkillsetSkill(AnalyticsBridgeSkillsetSkill analyticsBridgeSkillsetSkill);
	}
}