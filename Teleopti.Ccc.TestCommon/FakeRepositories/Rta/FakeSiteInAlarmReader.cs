using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeSiteInAlarmReader : ISiteInAlarmReader
	{
		private readonly INow _now;
		private readonly List<AgentStateReadModel> _data = new List<AgentStateReadModel>();
		private readonly List<personSkill> _personSkills = new List<personSkill>();
		private AgentStateReadModel lastAdded;

		public FakeSiteInAlarmReader(INow now)
		{
			_now = now;
		}

		public FakeSiteInAlarmReader Has(AgentStateReadModel model)
		{
			_data.Add(model);
			lastAdded = model;
			return this;
		}

		public FakeSiteInAlarmReader OnSkill(Guid skill)
		{
			_personSkills.Add(new personSkill
			{
				PersonId = lastAdded.PersonId,
				SkillId = skill,
			});
			return this;
		}

		public IEnumerable<SiteInAlarmModel> Read()
		{
			return
				_data
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
				from agentState in _data
				from agentSkill in _personSkills
				from skill in skillIds
				where agentSkill.SkillId == skill &&
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

		private class personSkill
		{
			public Guid PersonId { get; set; }
			public Guid SkillId { get; set; }
		}
	}
}
