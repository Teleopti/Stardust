using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
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
		private readonly IInitiatorIdentifierScope _initiatorIdentifierScope;

		public ServiceBusEventProcessor(
			CommonEventProcessor processor,
			ResolveEventHandlers resolver,
			IEventInfrastructureInfoPopulator eventInfrastructureInfoPopulator,
			ICurrentUnitOfWorkFactory unitOfWorkFactory,
			IInitiatorIdentifierScope initiatorIdentifierScope)
		{
			_processor = processor;
			_resolver = resolver;
			_eventInfrastructureInfoPopulator = eventInfrastructureInfoPopulator;
			_unitOfWorkFactory = unitOfWorkFactory;
			_initiatorIdentifierScope = initiatorIdentifierScope;
		}

		public void Process(IEvent @event)
		{
			var initiatorInfo = @event as IInitiatorContext;

			if (initiatorInfo == null)
				using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					_eventInfrastructureInfoPopulator.PopulateEventContext(@event);
					foreach (var handler in _resolver.HandlerTypesFor<IRunOnServiceBus>(@event))
						_processor.Process(@event, handler);
					unitOfWork.PersistAll();
				}
			else
				using (_initiatorIdentifierScope.OnThisThreadUse(new InitiatorIdentifierFromMessage(initiatorInfo)))
				{
					using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
					{
						_eventInfrastructureInfoPopulator.PopulateEventContext(@event);
						foreach (var handler in _resolver.HandlerTypesFor<IRunOnServiceBus>(@event))
							_processor.Process(@event, handler);
						unitOfWork.PersistAll();
					}
				}
		}

	}
}
