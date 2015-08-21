using System;
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

		public Interfaces.MessageBroker.Message CreateNotification(IAdherenceAggregatorInfo state)
		{
			var numberOfOutOfAdherence = _aggregationState.GetOutOfAdherenceForSite(state.Person.SiteId);
			return createSiteNotification(numberOfOutOfAdherence, state.Person.BusinessUnitId, state.Person.SiteId);
		}

		private Interfaces.MessageBroker.Message createSiteNotification(int numberOfOutOfAdherence, Guid businessUnitId, Guid siteId)
		{
			var siteAdherenceMessage = new SiteAdherenceMessage
			{
				OutOfAdherence = numberOfOutOfAdherence
			};

			return new Interfaces.MessageBroker.Message
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