using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.ApplicationLayer
{
	public class EventsConsumer : 
		ConsumerOf<IEvent>,
		ConsumerOf<EventsPackageMessage>
	{
		private readonly IEventPublisher _publisher;
		private readonly IServiceBus _bus;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public EventsConsumer(IEventPublisher publisher, IServiceBus bus, IUnitOfWorkFactory unitOfWorkFactory)
		{
			_publisher = publisher;
			_bus = bus;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public void Consume(IEvent message)
		{
			System.IO.File.AppendAllText("wtfdebug.log", "EventsConsumer " + message.GetType().Name);
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				_publisher.Publish(message);
				unitOfWork.PersistAll();
			}
			System.IO.File.AppendAllText("wtfdebug.log", "/EventsConsumer " + message.GetType().Name);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Consume(EventsPackageMessage message)
		{
			_bus.Send(message.Events);
		}
	}
}