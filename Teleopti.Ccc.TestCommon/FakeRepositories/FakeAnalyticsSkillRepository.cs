﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics.Transformer;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsSkillRepository : IAnalyticsSkillRepository
	{
		private List<AnalyticsSkill> fakeSkills;
		private List<AnalyticsSkillSet> fakeSkillSets;
		public List<AnalyticsBridgeSkillsetSkill> fakeBridgeSkillsetSkills; 

		public FakeAnalyticsSkillRepository()
		{
			fakeSkillSets = new List<AnalyticsSkillSet>();
			fakeBridgeSkillsetSkills = new List<AnalyticsBridgeSkillsetSkill>();
		}

		public IList<AnalyticsSkill> Skills(int businessUnitId)
		{
			return fakeSkills;
		}

		public int AddSkillSet(AnalyticsSkillSet analyticsSkillSet)
		{
			if (analyticsSkillSet.SkillsetId == 0)
			{
				analyticsSkillSet.SkillsetId = fakeSkillSets.Max(a => a.SkillsetId) + 1;
			}
			fakeSkillSets.Add(analyticsSkillSet);
			return analyticsSkillSet.SkillsetId;
		}

		public void AddBridgeSkillsetSkill(AnalyticsBridgeSkillsetSkill analyticsBridgeSkillsetSkill)
		{
			fakeBridgeSkillsetSkills.Add(analyticsBridgeSkillsetSkill);
		}

		public int AddSkillSet(List<AnalyticsSkill> listOfSkills)
		{
			var newSkillSet = PersonPeriodTransformer.NewSkillSetFromSkills(listOfSkills);
			fakeSkillSets.Add(newSkillSet);
			return newSkillSet.SkillsetId;
		}

		public IList<AnalyticsSkillSet> SkillSets()
		{
			return fakeSkillSets;
		}

		public int? SkillSetId(IList<AnalyticsSkill> skills)
		{
			var skillSet = fakeSkillSets.FirstOrDefault(a => a.SkillsetCode == string.Join(",", skills.Select(b => b.SkillId)));
			return skillSet == null ? null : (int?) skillSet.SkillsetId;
		}

		public void SetSkills(List<AnalyticsSkill> analyticsSkills)
		{
			fakeSkills = analyticsSkills;
		}

		public void SetSkillSets(List<AnalyticsSkillSet> list)
		{
			fakeSkillSets = list;
		}

		public void AddAgentSkill(int personId, int skillId, bool active, int businessUnitId)
		{
			throw new NotImplementedException();
		}

		public void DeleteAgentSkillForPersonId(int personId)
		{
			throw new NotImplementedException();
		}

		public IList<AnalyticsFactAgentSkill> GetFactAgentSkillsForPerson(int personId)
		{
			throw new NotImplementedException();
		}
	}
}