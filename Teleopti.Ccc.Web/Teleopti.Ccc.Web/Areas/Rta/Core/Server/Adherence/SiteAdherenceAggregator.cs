using System;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
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

		public Notification CreateNotification(IAdherenceAggregatorInfo state)
		{
			var numberOfOutOfAdherence = _aggregationState.GetOutOfAdherenceForSite(state.SiteId);
			return createSiteNotification(numberOfOutOfAdherence, state.NewState.BusinessUnitId, state.SiteId);
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