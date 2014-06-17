using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.Messages.Rta;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class AgentAdherenceAggregator
	{
		private readonly RtaAggregationStateProvider _stateProvider;

		public AgentAdherenceAggregator(RtaAggregationStateProvider stateProvider)
		{
			_stateProvider = stateProvider;
		}

		public Notification CreateNotification(IActualAgentState actualAgentState)
		{
			var aggregationState = _stateProvider.GetState();
			var organizationData = aggregationState.Update(actualAgentState.PersonId, actualAgentState);
			var actualAgentStateForTeam = aggregationState.GetActualAgentStateForTeam(organizationData.TeamId);
			var agentStates = actualAgentStateForTeam.Select(mapFrom);

			return createAgentsNotification(agentStates, actualAgentState.BusinessUnit, organizationData.TeamId);
		}

		private static AgentAdherenceStateInfo mapFrom(IActualAgentState actualAgentState)
		{
			return new AgentAdherenceStateInfo
			{
				PersonId = actualAgentState.PersonId,
				Activity = actualAgentState.Scheduled,
				Alarm = actualAgentState.AlarmName,
				AlarmColor = ColorTranslator.ToHtml(Color.FromArgb(actualAgentState.Color)),
				AlarmStart = actualAgentState.AlarmStart,
				NextActivity = actualAgentState.ScheduledNext,
				NextActivityStartTime = actualAgentState.NextStart,
				State = actualAgentState.State,
				StateStart = actualAgentState.StateStart
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
