using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Rta
{
    public class FakeTeamInAlarmReader : ITeamInAlarmReader
    {
        private readonly List<AgentStateReadModel> _data = new List<AgentStateReadModel>();
		
        public void Has(AgentStateReadModel model)
        {
			_data.Add(model);
        }

        public IEnumerable<TeamInAlarmModel> Read(Guid siteId)
        {
            return _data
				.Where(x => x.SiteId == siteId && x.IsRuleAlarm && x.AlarmStartTime == DateTime.MinValue)
				.GroupBy(x => x.TeamId)
				.Select(x => new TeamInAlarmModel
				{
					SiteId = siteId,
					TeamId = x.Key.Value,
					Count = x.Count()
				});
        }
    }
}