using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeSiteInAlarmReader : ISiteInAlarmReader
	{
		private readonly List<AgentStateReadModel> _data = new List<AgentStateReadModel>();

		public void Has(AgentStateReadModel model)
		{
			_data.Add(model);
		}

		public IEnumerable<SiteInAlarmModel> Read()
		{
			return
				_data
					.Where(x => x.IsRuleAlarm && x.AlarmStartTime == DateTime.MinValue)
					.GroupBy(x => x.SiteId)
					.Select(x => new SiteInAlarmModel
					{
						SiteId = x.Key.Value,
						Count = x.Count()
					});
		}
	}
}
