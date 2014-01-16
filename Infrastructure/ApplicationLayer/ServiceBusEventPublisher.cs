using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface IServiceBusEventPublisher : IEventPublisher
	{
		bool EnsureBus();
		void Publish(IRaptorDomainMessageInfo message);
		void PublishWithoutInitiatorInfo(IRaptorDomainMessageInfo message);
	}

	public class ServiceBusEventPublisher : IServiceBusEventPublisher
	{
		private readonly IServiceBusSender _sender;
		private readonly IEventContextPopulator _eventContextPopulator;

		public ServiceBusEventPublisher(IServiceBusSender sender, IEventContextPopulator eventContextPopulator)
		{
			_sender = sender;
			_eventContextPopulator = eventContextPopulator;
		}

		public void Publish(IEvent @event)
		{
			if (!_sender.EnsureBus())
				throw new ApplicationException("Cant find the bus, cant publish the event!");

			_eventContextPopulator.PopulateEventContext(@event);

			_sender.Send(@event);
		}
		
		public void Publish(IRaptorDomainMessageInfo message)
		{
			if (!_sender.EnsureBus())
				throw new ApplicationException("Cant find the bus, cant publish the event!");

			_eventContextPopulator.PopulateEventContext(message);

			_sender.Send(message);
		}

		public void PublishWithoutInitiatorInfo(IRaptorDomainMessageInfo message)
		{
			if (!_sender.EnsureBus())
				throw new ApplicationException("Cant find the bus, cant publish the event!");

			_eventContextPopulator.PopulateEventContextWithoutInitiator(message);

			_sender.Send(message);
		}

		public bool EnsureBus()
		{
			return _sender.EnsureBus();
		}

	}
}