using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsSkillRepository
	{
		IList<AnalyticsSkillSet> SkillSets();
		int? SkillSetId(IList<AnalyticsSkill> skills);
		IEnumerable<AnalyticsSkill> Skills(int businessUnitId);
		int AddSkillSet(AnalyticsSkillSet analyticsSkillSet);
		void AddBridgeSkillsetSkill(AnalyticsBridgeSkillsetSkill analyticsBridgeSkillsetSkill);

		// Fact agent skill
		void AddAgentSkill(int personId, int skillId, bool active, int businessUnitId);
		void DeleteAgentSkillForPersonId(int personId);
		void AddOrUpdateSkill(AnalyticsSkill analyticsSkill);
	}
}