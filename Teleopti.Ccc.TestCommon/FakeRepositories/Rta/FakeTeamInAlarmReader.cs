using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
    public class FakeTeamInAlarmReader : ITeamInAlarmReader
    {
	    private readonly INow _now;
	    private readonly List<AgentStateReadModel> _data = new List<AgentStateReadModel>();
		private readonly List<personSkill> _personSkills = new List<personSkill>();
		private AgentStateReadModel lastAdded;

	    public FakeTeamInAlarmReader(INow now)
	    {
		    _now = now;
	    }

	    public void Has(AgentStateReadModel model)
        {
			_data.Add(model);
			lastAdded = model;
		}

		public void OnSkill(Guid skill)
		{
			_personSkills.Add(new personSkill
			{
				PersonId = lastAdded.PersonId,
				SkillId = skill,
			});
		}

		public IEnumerable<TeamInAlarmModel> Read(Guid siteId)
		{
			return _data
				.Where(x =>
					x.SiteId == siteId &&
					x.IsRuleAlarm &&
					x.AlarmStartTime <= _now.UtcDateTime())
				.GroupBy(x => x.TeamId)
				.Select(x => new TeamInAlarmModel
				{
					SiteId = siteId,
					TeamId = x.Key.Value,
					Count = x.Count()
				});
		}

	    public IEnumerable<TeamInAlarmModel> ReadForSkills(Guid siteId, Guid[] skillIds)
	    {
			return
				from agentState in _data
				from agentSkill in _personSkills
				from skill in skillIds
				where agentSkill.SkillId == skill &&
					  agentSkill.PersonId == agentState.PersonId &&
					  agentState.SiteId == siteId &&
					  _now.UtcDateTime() >= agentState.AlarmStartTime &&
					  agentState.IsRuleAlarm
				group agentState by agentState.TeamId
				into g
				select new TeamInAlarmModel
				{
					SiteId = siteId,
					TeamId = g.Key.GetValueOrDefault(),
					Count = g.Select(x => x.PersonId).Distinct().Count()
				};
			
	    }
		
		private class personSkill
		{
			public Guid PersonId { get; set; }
			public Guid SkillId { get; set; }
		}
	}
}