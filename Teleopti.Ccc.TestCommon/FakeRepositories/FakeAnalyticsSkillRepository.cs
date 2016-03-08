using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsSkillRepository : IAnalyticsSkillRepository
	{
		private List<AnalyticsSkill> fakeSkills;
		private List<KeyValuePair<int, string>> fakeSkillSets;

		public IList<AnalyticsSkill> Skills(int businessUnitId)
		{
			return fakeSkills;
		}

		public int? SkillSetId(IList<AnalyticsSkill> skills)
		{
			var skillSet = fakeSkillSets.FirstOrDefault(a => a.Value == string.Join(",", skills.Select(b => b.SkillId)));
			if (!skillSet.Equals(default(KeyValuePair<int, string>)))
				return skillSet.Key;
			return null;
		}

		public void SetSkills(List<AnalyticsSkill> analyticsSkills)
		{
			fakeSkills = analyticsSkills;
		}

		public void SetSkillSets(List<KeyValuePair<int, string>> list)
		{
			fakeSkillSets = list;
		}
	}
}