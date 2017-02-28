using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
#pragma warning disable 618

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
			ICurrentUnitOfWorkFactory unitOfWorkFactory
			)
		{
			_processor = processor;
			_resolver = resolver;
			_eventInfrastructureInfoPopulator = eventInfrastructureInfoPopulator;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public void Process(IEvent @event)
		{
			Process(@event, _resolver.HandlerTypesFor<IRunOnServiceBus>(@event));
		}

		public void Process(IEvent @event, IEnumerable<Type> handlerTypes)
		{
			_eventInfrastructureInfoPopulator.PopulateEventContext(@event);

			using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				foreach (var handler in handlerTypes)
					_processor.Process(@event, handler);
				unitOfWork.PersistAll(InitiatorIdentifier.FromMessage(@event));
			}
		}
	}
#pragma warning restore 618

}
