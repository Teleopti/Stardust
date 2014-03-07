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
		private readonly ITeamIdForPerson _teamProvider;
		private readonly ISiteIdForPerson _siteProvider;
		private readonly Dictionary<Guid, TeamAdherence> _teamAdherence = new Dictionary<Guid, TeamAdherence>();

		public AdherenceAggregator(IMessageSender messageSender, ITeamIdForPerson teamProvider, ISiteIdForPerson siteProvider)
		{
			_messageSender = messageSender;
			_teamProvider = teamProvider;
			_siteProvider = siteProvider;
		}

		public void Invoke(IActualAgentState actualAgentState)
		{
			var personId = actualAgentState.PersonId;

			if (_teamProvider == null)
			{
				var siteAdherenceMessage = new SiteAdherenceMessage
				{
					SiteId = Guid.Empty,
					OutOfAdherence = 2
				};
				var notification2 = new Notification { BinaryData = JsonConvert.SerializeObject(siteAdherenceMessage) };
				_messageSender.SendNotification(notification2);
			}
			if (_siteProvider == null)
			{
				var teamId = _teamProvider.GetTeamId(personId);

				if (!_teamAdherence.ContainsKey(teamId))
					_teamAdherence[teamId] = new TeamAdherence();

				var teamState = _teamAdherence[teamId];
				var changed = teamState.TryUpdateAdherence(personId, actualAgentState.StaffingEffect);
				if (!changed)
					return;

				var teamAdherenceMessage = new TeamAdherenceMessage
				{
					TeamId = teamId,
					OutOfAdherence = teamState.NumberOutOfAdherence()
				};
				var notification = new Notification { BinaryData = JsonConvert.SerializeObject(teamAdherenceMessage) };
				_messageSender.SendNotification(notification);
			}
			
		}
	}
}