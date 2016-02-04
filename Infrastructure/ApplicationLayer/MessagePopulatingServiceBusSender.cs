using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class MessagePopulatingServiceBusSender : IMessagePopulatingServiceBusSender
	{
		private readonly IServiceBusSender _serviceBus;
		private readonly IEventInfrastructureInfoPopulator _eventInfrastructureInfoPopulator;

		public MessagePopulatingServiceBusSender(IServiceBusSender serviceBus, IEventInfrastructureInfoPopulator eventInfrastructureInfoPopulator)
		{
			_serviceBus = serviceBus;
			_eventInfrastructureInfoPopulator = eventInfrastructureInfoPopulator;
		}

		public void Send(object message, bool throwOnNoBus)
		{
			_eventInfrastructureInfoPopulator.PopulateEventContext(message);
			_serviceBus.Send(throwOnNoBus, message);
		}
	}
}