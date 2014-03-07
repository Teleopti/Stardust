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
		private readonly Dictionary<Guid, SiteAdherence> _siteAdherence = new Dictionary<Guid, SiteAdherence>();

		public AdherenceAggregator(IMessageSender messageSender, ITeamIdForPerson teamProvider, ISiteIdForPerson siteProvider)
		{
			_messageSender = messageSender;
			_teamProvider = teamProvider;
			_siteProvider = siteProvider;
		}

		public void Invoke(IActualAgentState actualAgentState)
		{
			var personId = actualAgentState.PersonId;
			aggregateSiteAdherence(actualAgentState, personId);
			aggregateTeamAdherence(actualAgentState, personId);
		}

		private void aggregateSiteAdherence(IActualAgentState actualAgentState, Guid personId)
		{
			if (_siteProvider == null) return;

			var siteId = _siteProvider.GetSiteId(personId);
			if (!_siteAdherence.ContainsKey(siteId))
				_siteAdherence[siteId] = new SiteAdherence();

			var teamState = _siteAdherence[siteId];
			var changed = teamState.TryUpdateAdherence(personId, actualAgentState.StaffingEffect);
			if (!changed)
				return;
			var siteAdherenceMessage = new SiteAdherenceMessage
			{
				SiteId = siteId,
				OutOfAdherence = teamState.NumberOutOfAdherence()
			};
			var notification = new Notification { BinaryData = JsonConvert.SerializeObject(siteAdherenceMessage) };
			_messageSender.SendNotification(notification);
		}

		private void aggregateTeamAdherence(IActualAgentState actualAgentState, Guid personId)
		{
			if (_teamProvider == null) return;

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
			var notification = new Notification {BinaryData = JsonConvert.SerializeObject(teamAdherenceMessage)};
			_messageSender.SendNotification(notification);
		}
	}
}