using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Rta;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.Aggregator
{
	public class AgentAdherenceAggregator
	{
		private readonly AggregationState _aggregationState;
		private readonly IJsonSerializer _jsonSerializer;

		public AgentAdherenceAggregator(AggregationState aggregationState, IJsonSerializer jsonSerializer)
		{
			_aggregationState = aggregationState;
			_jsonSerializer = jsonSerializer;
		}

		public IEnumerable<Message> CreateNotification(IAdherenceAggregatorInfo state)
		{
			var actualAgentStateForTeam = _aggregationState.GetActualAgentStateForTeam(state.Person.TeamId);
			var agentStates = actualAgentStateForTeam.Select(mapFrom);
			return
				agentStates.Batch(40)
					.Select(s => createAgentsNotification(s, state.Person.BusinessUnitId, state.Person.TeamId));
		}

		private static AgentAdherenceStateInfo mapFrom(AgentStateReadModel agentStateReadModel)
		{
			return new AgentAdherenceStateInfo
			{
				PersonId = agentStateReadModel.PersonId,
				Activity = agentStateReadModel.Scheduled,
				Alarm = agentStateReadModel.AlarmName,
				AlarmColor = ColorTranslator.ToHtml(Color.FromArgb(agentStateReadModel.Color ?? 0)),
				RuleStartTime = agentStateReadModel.RuleStartTime,
				NextActivity = agentStateReadModel.ScheduledNext,
				NextActivityStartTime = agentStateReadModel.NextStart,
				State = agentStateReadModel.State,
				StateStartTime = agentStateReadModel.StateStartTime
			};
		}

		private Message createAgentsNotification(IEnumerable<AgentAdherenceStateInfo> agentStates, Guid businessUnitId, Guid teamId)
		{
			var agentsAdherenceMessage = new AgentsAdherenceMessage { AgentStates = agentStates };

			return new Message
			{
				BinaryData = _jsonSerializer.SerializeObject(agentsAdherenceMessage),
				BusinessUnitId = businessUnitId.ToString(),
				DomainType = "AgentsAdherenceMessage",
				DomainId = teamId.ToString(),
				DomainReferenceId = teamId.ToString()
			};
		}
	}
}
