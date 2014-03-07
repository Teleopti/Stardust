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
			_teamAdherenceAggregator = new TeamAdherenceAggregator(_messageSender, teamProvider);
			_siteAdherenceAggregator = new SiteAdherenceAggregator(_messageSender, siteProvider);
		}

		public void Invoke(IActualAgentState actualAgentState)
		{
			_siteAdherenceAggregator.Aggregate(actualAgentState);
			_teamAdherenceAggregator.Aggregate(actualAgentState);
		}
	}

	public class TeamAdherenceAggregator
	{
		private readonly IMessageSender _messageSender;
		private readonly ITeamIdForPerson _teamProvider;
		private readonly Dictionary<Guid, TeamAdherence> _teamAdherence = new Dictionary<Guid, TeamAdherence>();

		public TeamAdherenceAggregator(IMessageSender messageSender, ITeamIdForPerson teamProvider)
		{
			_messageSender = messageSender;
			_teamProvider = teamProvider;
		}

		public void Aggregate(IActualAgentState actualAgentState)
		{
			if (_teamProvider == null) return;
			var personId = actualAgentState.PersonId;
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
	}
}