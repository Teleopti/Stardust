using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeSiteInAlarmReader : ISiteInAlarmReader
	{
		private readonly INow _now;
		private readonly FakeAgentStateReadModelPersister _agentStates;
		private readonly IGroupingReadOnlyRepository _groupings;
		private readonly HardcodedSkillGroupingPageId _hardcodedSkillGroupingPageId;

		public FakeSiteInAlarmReader(
			FakeAgentStateReadModelPersister agentStates, 
			IGroupingReadOnlyRepository groupings,
			HardcodedSkillGroupingPageId hardcodedSkillGroupingPageId, 
			INow now)
		{
			_now = now;
			_agentStates = agentStates;
			_groupings = groupings;
			_hardcodedSkillGroupingPageId = hardcodedSkillGroupingPageId;
		}
		
		public IEnumerable<SiteInAlarmModel> Read()
		{
			return
				_agentStates.Models 
					.Where(x => x.IsRuleAlarm && x.AlarmStartTime <= _now.UtcDateTime())
					.GroupBy(x => x.SiteId)
					.Select(x => new SiteInAlarmModel
					{
						SiteId = x.Key.Value,
						Count = x.Count()
					});
		}

		public IEnumerable<SiteInAlarmModel> ReadForSkills(Guid[] skillIds)
		{
			
			return
				from agentState in _agentStates.Models
				let today = new DateOnly(_now.UtcDateTime())
				from agentSkill in _groupings.DetailsForGroup(_hardcodedSkillGroupingPageId.GetGuid(), today)
				from skill in skillIds
				where agentSkill.GroupId == skill &&
					  agentSkill.PersonId == agentState.PersonId &&
					  _now.UtcDateTime() >= agentState.AlarmStartTime &&
					  agentState.IsRuleAlarm
				group agentState by agentState.SiteId
				into g
				select new SiteInAlarmModel
				{
					SiteId = g.Key.GetValueOrDefault(),
					Count = g.Select(x => x.PersonId).Distinct().Count()
				};

		}
	}
}
