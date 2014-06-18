using System;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class SiteAdherenceAggregator
	{
		private readonly RtaAggregationStateProvider _stateProvider;

		public SiteAdherenceAggregator(RtaAggregationStateProvider stateProvider)
		{
			_stateProvider = stateProvider;
		}

		public Notification CreateNotification(IActualAgentState actualAgentState)
		{
			var aggregationState = _stateProvider.GetState();
			var personOrganizationData = aggregationState.Update(actualAgentState.PersonId, actualAgentState);
			var numberOfOutOfAdherence = aggregationState.GetActualAgentStateForSite(personOrganizationData.SiteId).Count(x => Math.Abs(x.StaffingEffect) > 0.01);

			return createSiteNotification(numberOfOutOfAdherence, actualAgentState.BusinessUnit, personOrganizationData.SiteId);
		}

		private static Notification createSiteNotification(int numberOfOutOfAdherence, Guid businessUnitId, Guid siteId)
		{
			var siteAdherenceMessage = new SiteAdherenceMessage
			{
				OutOfAdherence = numberOfOutOfAdherence
			};

			return new Notification
			{
				BinaryData = JsonConvert.SerializeObject(siteAdherenceMessage),
				BusinessUnitId = businessUnitId.ToString(),
				DomainType = "SiteAdherenceMessage",
				DomainId = siteId.ToString(),
				DomainReferenceId = siteId.ToString()
			};
		}
	}
}