using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class SiteAdherenceAggregator
	{
		private readonly IOrganizationForPerson _organizationForPerson;
		private readonly Dictionary<Guid, AggregatedValues> _siteAdherences = new Dictionary<Guid, AggregatedValues>();

		public SiteAdherenceAggregator(IOrganizationForPerson organizationForPerson)
		{
			_organizationForPerson = organizationForPerson;
		}

		public Notification CreateNotification(IActualAgentState actualAgentState)
		{
			if (_organizationForPerson == null) return null;

			var personId = actualAgentState.PersonId;
			var siteId = _organizationForPerson.GetOrganization(personId).SiteId;

			AggregatedValues siteState;
			if (!_siteAdherences.TryGetValue(siteId, out siteState))
			{
				siteState = new AggregatedValues(siteId);
				_siteAdherences[siteId] = siteState;
			}

			var changed = siteState.TryUpdateAdherence(personId, actualAgentState.StaffingEffect);
			return changed
				? createSiteNotification(_siteAdherences[siteId], actualAgentState.BusinessUnit)
				: null;
		}

		private static Notification createSiteNotification(AggregatedValues aggregatedValues, Guid businessUnitId)
		{
			var siteAdherenceMessage = new SiteAdherenceMessage
			{
				OutOfAdherence = aggregatedValues.NumberOutOfAdherence()
			};
			return new Notification
			{
				BinaryData = JsonConvert.SerializeObject(siteAdherenceMessage),
				BusinessUnitId = businessUnitId.ToString(),
				DomainType = "SiteAdherenceMessage",
				DomainId = aggregatedValues.Key.ToString()
			};
			
		}
	}
}