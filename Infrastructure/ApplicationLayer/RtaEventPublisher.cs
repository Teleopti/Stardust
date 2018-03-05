using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class RtaEventPublisher : IEventPublisher
	{
		private readonly IRtaEventStore _store;

		public RtaEventPublisher(IRtaEventStore store)
		{
			_store = store;
		}
		
		public void Publish(params IEvent[] events)
		{
			events.ForEach(_store.Add);
		}
	}
}