using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModelBuilders;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public interface IGetAgentStates
	{
		IEnumerable<AgentStatusViewModel> ForSites(Guid[] siteIds, bool? inAlarmOnly = null, bool? alarmTimeDesc = null);
		IEnumerable<AgentStatusViewModel> ForTeams(Guid[] teamIds, bool? inAlarmOnly = null, bool? alarmTimeDesc = null);
	}

	public class GetAgentStates : IGetAgentStates
	{
		private readonly IAgentStateReadModelReader _agentStateReadModelReader;
		private readonly IAgentStateViewModelBuilder _agentStateViewModelBuilder;

		public GetAgentStates(IAgentStateReadModelReader agentStateReadModelReader,
			IAgentStateViewModelBuilder agentStateViewModelBuilder)
		{
			_agentStateReadModelReader = agentStateReadModelReader;
			_agentStateViewModelBuilder = agentStateViewModelBuilder;
		}

		public IEnumerable<AgentStatusViewModel> ForSites(Guid[] siteIds, bool? inAlarmOnly, bool? alarmTimeDesc)
		{
			return _agentStateViewModelBuilder.Build(_agentStateReadModelReader.LoadForSites(siteIds, inAlarmOnly, alarmTimeDesc));
		}

		public IEnumerable<AgentStatusViewModel> ForTeams(Guid[] teamIds, bool? inAlarmOnly, bool? alarmTimeDesc)
		{
			return _agentStateViewModelBuilder.Build(_agentStateReadModelReader.LoadForTeams(teamIds, inAlarmOnly));
		}
	}
}