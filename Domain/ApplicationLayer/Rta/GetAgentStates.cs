using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModelBuilders;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IGetAgentStates
	{
		IEnumerable<AgentStateViewModel> ForSites(Guid[] siteIds, bool inAlarm);
		IEnumerable<AgentStateViewModel> ForTeams(Guid[] teamIds, bool inAlarm);
	}

	public class GetAgentStates : IGetAgentStates
	{
		private readonly IAgentStateReadModelReader _agentStateReadModelReader;
		private readonly IAgentStateViewModelBuilder _agentStateViewModelBuilder;

		public GetAgentStates(
			IAgentStateReadModelReader agentStateReadModelReader,
			IAgentStateViewModelBuilder agentStateViewModelBuilder
			)
		{
			_agentStateReadModelReader = agentStateReadModelReader;
			_agentStateViewModelBuilder = agentStateViewModelBuilder;
		}

		public IEnumerable<AgentStateViewModel> ForSites(Guid[] siteIds, bool inAlarm)
		{
			return _agentStateViewModelBuilder.Build(_agentStateReadModelReader.LoadForSites(siteIds, inAlarm));
		}

		public IEnumerable<AgentStateViewModel> ForTeams(Guid[] teamIds, bool inAlarm)
		{
			return _agentStateViewModelBuilder.Build(_agentStateReadModelReader.LoadForTeams(teamIds, inAlarm));
		}
	}
}