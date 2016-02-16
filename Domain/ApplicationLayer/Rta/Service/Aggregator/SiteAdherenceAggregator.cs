using System;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.Aggregator
{
	public class SiteAdherenceAggregator
	{
		private readonly AggregationState _aggregationState;
		private readonly IJsonSerializer _jsonSerializer;

		public SiteAdherenceAggregator(AggregationState aggregationState, IJsonSerializer jsonSerializer)
		{
			_aggregationState = aggregationState;
			_jsonSerializer = jsonSerializer;
		}

		public Message CreateNotification(IAdherenceAggregatorInfo state)
		{
			var numberOfOutOfAdherence = _aggregationState.GetOutOfAdherenceForSite(state.Person.SiteId);
			return createSiteNotification(numberOfOutOfAdherence, state.Person.BusinessUnitId, state.Person.SiteId);
		}

		private Message createSiteNotification(int numberOfOutOfAdherence, Guid businessUnitId, Guid siteId)
		{
			var siteAdherenceMessage = new SiteAdherenceMessage
			{
				OutOfAdherence = numberOfOutOfAdherence
			};

			return new Message
			{
				BinaryData = _jsonSerializer.SerializeObject(siteAdherenceMessage),
				BusinessUnitId = businessUnitId.ToString(),
				DomainType = "SiteAdherenceMessage",
				DomainId = siteId.ToString(),
				DomainReferenceId = siteId.ToString()
			};
		}
	}
}