using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class AdherenceAggregator : IAfterSend
	{
		private readonly IMessageSender _messageSender;
		private readonly ITeamIdForPersonProvider _teamProvider;
		private readonly Dictionary<Guid, TeamAdherence> _teamAdherence = new Dictionary<Guid, TeamAdherence>();  

		public AdherenceAggregator(IMessageSender messageSender, ITeamIdForPersonProvider teamProvider)
		{
			_messageSender = messageSender;
			_teamProvider = teamProvider;
		}

		public void Invoke(IActualAgentState actualAgentState)
		{
			var personId = actualAgentState.PersonId;
			var teamId = _teamProvider.GetTeamId(personId);

			if (!_teamAdherence.ContainsKey(teamId))
				_teamAdherence[teamId] = new TeamAdherence();

			var teamState = _teamAdherence[teamId];
			var changed = teamState.TryUpdateAdherence(personId, actualAgentState.StaffingEffect);
			if (!changed)
				return;

			var teamAdherenceMessage = new TeamAdherenceMessage { TeamId = teamId, OutOfAdherence = teamState.NumberOutOfAdherence() };
			var notification = new Notification {BinaryData = JsonConvert.SerializeObject(teamAdherenceMessage)};
			_messageSender.SendNotification(notification);
		}
	}
}