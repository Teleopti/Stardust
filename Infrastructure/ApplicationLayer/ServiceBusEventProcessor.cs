using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class ServiceBusEventProcessor
	{
		private readonly CommonEventProcessor _processor;
		private readonly ResolveEventHandlers _resolver;
		private readonly IEventInfrastructureInfoPopulator _eventInfrastructureInfoPopulator;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		public ServiceBusEventProcessor(
			CommonEventProcessor processor,
			ResolveEventHandlers resolver,
			IEventInfrastructureInfoPopulator eventInfrastructureInfoPopulator,
			ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_processor = processor;
			_resolver = resolver;
			_eventInfrastructureInfoPopulator = eventInfrastructureInfoPopulator;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public void Process(IEvent @event)
		{
			_eventInfrastructureInfoPopulator.PopulateEventContext(@event);

			var initiatorInfo = @event as IInitiatorContext;

			if (initiatorInfo == null)
				using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					foreach (var handler in _resolver.ResolveServiceBusHandlersForEvent(@event))
						_processor.Process(@event, handler);
					unitOfWork.PersistAll();
				}
			else
				using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork(new InitiatorIdentifierFromMessage(initiatorInfo)))
				{
					foreach (var handler in _resolver.ResolveServiceBusHandlersForEvent(@event))
						_processor.Process(@event, handler);
					unitOfWork.PersistAll();
				}
		}

	}
}
