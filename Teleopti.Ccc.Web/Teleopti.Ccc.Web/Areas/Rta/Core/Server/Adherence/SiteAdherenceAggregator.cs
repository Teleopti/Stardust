using System;
using Newtonsoft.Json;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence
{
	public class SiteAdherenceAggregator
	{
		private readonly AggregationState _aggregationState;

		public SiteAdherenceAggregator(AggregationState aggregationState)
		{
			_aggregationState = aggregationState;
		}

		public Notification CreateNotification(PersonOrganizationData personOrganizationData, IActualAgentState actualAgentState)
		{
			var numberOfOutOfAdherence = _aggregationState.GetOutOfAdherenceForSite(personOrganizationData.SiteId);
			return createSiteNotification(numberOfOutOfAdherence, actualAgentState.BusinessUnitId, personOrganizationData.SiteId);
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