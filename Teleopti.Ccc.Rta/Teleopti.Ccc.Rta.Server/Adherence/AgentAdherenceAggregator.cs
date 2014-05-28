using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.Messages.Rta;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class AgentAdherenceAggregator
	{
		private readonly IOrganizationForPerson _organizationForPerson;
		private readonly Dictionary<Guid, AgentStatesForOneTeam> _agentsAdherences = new Dictionary<Guid, AgentStatesForOneTeam>();

		public AgentAdherenceAggregator(IOrganizationForPerson organizationForPerson)
		{
			_organizationForPerson = organizationForPerson;
		}

		public Notification CreateNotification(IActualAgentState actualAgentState)
		{
			if (_organizationForPerson == null) return null;

			var personId = actualAgentState.PersonId;
			var personOrganizationData = _organizationForPerson.GetOrganization(personId);
			var teamId = personOrganizationData.TeamId;

			AgentStatesForOneTeam agentStatesForOneTeam;
			if (!_agentsAdherences.TryGetValue(teamId, out agentStatesForOneTeam))
			{
				agentStatesForOneTeam = new AgentStatesForOneTeam();
				_agentsAdherences[teamId] = agentStatesForOneTeam;
			}
			var changed = agentStatesForOneTeam.TryUpdateAdherence(personId, actualAgentState);
			return changed
				? createAgentsNotification(_agentsAdherences[teamId], actualAgentState.BusinessUnit, personOrganizationData.TeamId)
				: null;
		}


		private static Notification createAgentsNotification(AgentStatesForOneTeam agentStatesForOneTeam, Guid businessUnitId, Guid teamId)
		{
			var agentsAdherenceMessage = new AgentsAdherenceMessage {AgentStates = agentStatesForOneTeam.AgentStates};

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

	public class AgentStatesForOneTeam
	{
		private readonly IDictionary<Guid, AgentAdherenceStateInfo> _agentStates = new Dictionary<Guid, AgentAdherenceStateInfo>();

		public bool TryUpdateAdherence(Guid personId, IActualAgentState actualAgentState)
		{
			AgentAdherenceStateInfo agentStateInfo;
			if (!_agentStates.TryGetValue(personId, out agentStateInfo))
			{
				_agentStates[personId] = mapFrom(actualAgentState);
				return true;
			}
			var newState = mapFrom(actualAgentState);
			_agentStates[personId] = newState;
			return !newState.Equals(agentStateInfo);
		}

		public IEnumerable<AgentAdherenceStateInfo> AgentStates
		{
			get { return _agentStates.Values; }
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
				State = actualAgentState.State
			};
		}
	}
}
