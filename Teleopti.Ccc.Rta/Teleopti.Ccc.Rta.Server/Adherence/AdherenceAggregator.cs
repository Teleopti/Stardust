using System;
using System.Collections.Generic;
using System.Linq;
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
		private readonly Dictionary<Guid, double> _teamAdherence = new Dictionary<Guid, double>();  

		public AdherenceAggregator(IMessageSender messageSender, ITeamIdForPersonProvider teamProvider)
		{
			_messageSender = messageSender;
			_teamProvider = teamProvider;
		}

		public void Invoke(IActualAgentState actualAgentState)
		{
			var personId = actualAgentState.PersonId;
			var teamId = _teamProvider.GetTeamId(personId);

			double staffingEffect;
			if (_teamAdherence.TryGetValue(personId, out staffingEffect) &&
			    staffingEffect.Equals(actualAgentState.StaffingEffect))
				return;
			
			_teamAdherence[personId] = Math.Abs(actualAgentState.StaffingEffect);

			var outOfAdherenceSum = _teamAdherence.Values.Sum();

			var teamAdherenceMessage = new TeamAdherenceMessage { TeamId = teamId, OutOfAdherence = outOfAdherenceSum };
			var notification = new Notification {BinaryData = JsonConvert.SerializeObject(teamAdherenceMessage)};
			_messageSender.SendNotification(notification);
		}
	}
}