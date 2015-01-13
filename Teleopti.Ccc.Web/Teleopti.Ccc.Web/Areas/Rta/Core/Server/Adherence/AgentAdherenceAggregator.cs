using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.Messages.Rta;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence
{
	public class AgentAdherenceAggregator
	{
		private readonly AggregationState _aggregationState;

		public AgentAdherenceAggregator(AggregationState aggregationState)
		{
			_aggregationState = aggregationState;
		}

		public IEnumerable<Notification> CreateNotification(IAdherenceAggregatorInfo state)
		{
			var actualAgentStateForTeam = _aggregationState.GetActualAgentStateForTeam(state.TeamId);
			var agentStates = actualAgentStateForTeam.Select(mapFrom);
			return
				agentStates.Batch(40)
					.Select(s => createAgentsNotification(s, state.BusinessUnitId, state.TeamId));
		}

		private static AgentAdherenceStateInfo mapFrom(AgentStateReadModel agentStateReadModel)
		{
			return new AgentAdherenceStateInfo
			{
				PersonId = agentStateReadModel.PersonId,
				Activity = agentStateReadModel.Scheduled,
				Alarm = agentStateReadModel.AlarmName,
				AlarmColor = ColorTranslator.ToHtml(Color.FromArgb(agentStateReadModel.Color ?? 0)),
				AlarmStart = agentStateReadModel.AlarmStart,
				NextActivity = agentStateReadModel.ScheduledNext,
				NextActivityStartTime = agentStateReadModel.NextStart,
				State = agentStateReadModel.State,
				StateStart = agentStateReadModel.StateStart
			};
		}

		private static Notification createAgentsNotification(IEnumerable<AgentAdherenceStateInfo> agentStates, Guid businessUnitId, Guid teamId)
		{
			var agentsAdherenceMessage = new AgentsAdherenceMessage { AgentStates = agentStates };

			return new Notification
			{
				BinaryData = JsonConvert.SerializeObject(agentsAdherenceMessage),
				BusinessUnitId = businessUnitId.ToString(),
				DomainType = "AgentsAdherenceMessage",
				DomainId = teamId.ToString(),
				DomainReferenceId = teamId.ToString()
			};
		}
	}
}
