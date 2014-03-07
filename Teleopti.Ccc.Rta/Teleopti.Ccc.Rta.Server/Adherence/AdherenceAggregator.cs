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
		private readonly TeamAdherenceAggregator _teamAdherenceAggregator;
		private readonly SiteAdherenceAggregator _siteAdherenceAggregator;

		public AdherenceAggregator(IMessageSender messageSender, ITeamIdForPerson teamProvider, ISiteIdForPerson siteProvider)
		{
			_messageSender = messageSender;
			_teamAdherenceAggregator = new TeamAdherenceAggregator(teamProvider);
			_siteAdherenceAggregator = new SiteAdherenceAggregator(_messageSender, siteProvider);
		}

		public void Invoke(IActualAgentState actualAgentState)
		{
			_siteAdherenceAggregator.Aggregate(actualAgentState);
			var teamAdherence = _teamAdherenceAggregator.Aggregate(actualAgentState);
			if (teamAdherence == null) return;
			var notification = createTeamNotification(teamAdherence);
			_messageSender.SendNotification(notification);
		}

		private static Notification createTeamNotification(TeamAdherence teamAdherence)
		{
			var teamAdherenceMessage = new TeamAdherenceMessage
				{
					TeamId = teamAdherence.TeamId,
					OutOfAdherence = teamAdherence.NumberOutOfAdherence()
				};
			var notification = new Notification {BinaryData = JsonConvert.SerializeObject(teamAdherenceMessage)};
			return notification;
		}
	}

	public class SiteAdherenceAggregator
	{
		private readonly IMessageSender _messageSender;
		private readonly ISiteIdForPerson _siteProvider;
		private readonly Dictionary<Guid, SiteAdherence> _siteAdherence = new Dictionary<Guid, SiteAdherence>();

		public SiteAdherenceAggregator(IMessageSender messageSender, ISiteIdForPerson siteProvider)
		{
			_messageSender = messageSender;
			_siteProvider = siteProvider;
		}

		public void Aggregate(IActualAgentState actualAgentState)
		{
			if (_siteProvider == null) return;

			var personId = actualAgentState.PersonId;
			var siteId = _siteProvider.GetSiteId(personId);
			if (!_siteAdherence.ContainsKey(siteId))
				_siteAdherence[siteId] = new SiteAdherence();

			var siteState = _siteAdherence[siteId];
			var changed = siteState.TryUpdateAdherence(personId, actualAgentState.StaffingEffect);
			if (!changed)
				return;
			var siteAdherenceMessage = new SiteAdherenceMessage
			{
				SiteId = siteId,
				OutOfAdherence = siteState.NumberOutOfAdherence()
			};
			var notification = new Notification { BinaryData = JsonConvert.SerializeObject(siteAdherenceMessage) };
			_messageSender.SendNotification(notification);
		}
	}
}