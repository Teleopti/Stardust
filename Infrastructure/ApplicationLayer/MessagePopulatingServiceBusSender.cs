using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class MessagePopulatingServiceBusSender : IMessagePopulatingServiceBusSender
	{
		private readonly IServiceBusSender _serviceBus;
		private readonly IEventContextPopulator _eventContextPopulator;

		public MessagePopulatingServiceBusSender(IServiceBusSender serviceBus, IEventContextPopulator eventContextPopulator)
		{
			_serviceBus = serviceBus;
			_eventContextPopulator = eventContextPopulator;
		}

		public void Send(object message, bool throwOnNoBus)
		{
			_eventContextPopulator.PopulateEventContext(message);
			_serviceBus.Send(message, throwOnNoBus);
		}
	}
}