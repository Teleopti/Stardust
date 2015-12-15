using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModelBuilders;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public interface IGetAgentStates
	{
		IEnumerable<AgentStatusViewModel> ForSites(Guid[] siteIds);
		IEnumerable<AgentStatusViewModel> ForTeams(Guid[] teamIds);
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

		public IEnumerable<AgentStatusViewModel> ForSites(Guid[] siteIds)
		{
			return _agentStateViewModelBuilder.Build(_agentStateReadModelReader.LoadForSites(siteIds));
		}

		public IEnumerable<AgentStatusViewModel> ForTeams(Guid[] teamIds)
		{
			return _agentStateViewModelBuilder.Build(_agentStateReadModelReader.LoadForTeams(teamIds));
		}
	}
}