using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.ServiceBus.ApplicationLayer;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.ApplicationLayer
{
	[TestFixture]
	public class EventsConsumerTest
	{
		[Test]
		public void ShouldPublishEvents()
		{
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(MockRepository.GenerateMock<IUnitOfWork>());
			currentUnitOfWorkFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(unitOfWorkFactory);
			var publisher = MockRepository.GenerateMock<IEventPublisher>();
			var target = new EventsConsumer(publisher, null, currentUnitOfWorkFactory);
			var @event = new Event();

			target.Consume(@event);

			publisher.AssertWasCalled(x => x.Publish(@event));
		}

		[Test]
		public void ShouldCreateAndPersistUnitOfWork()
		{
            var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
            var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
            var currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
            unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            currentUnitOfWorkFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(unitOfWorkFactory);
			var target = new EventsConsumer(MockRepository.GenerateMock<IEventPublisher>(), null, currentUnitOfWorkFactory);

			target.Consume(new Event());

			unitOfWork.AssertWasCalled(x => x.PersistAll());
		}

		[Test]
		public void ShouldSendEventsFromPackageMessage()
		{
			var bus = MockRepository.GenerateMock<IServiceBus>();
			var target = new EventsConsumer(null, bus, null);
			var message = new EventsPackageMessage {Events = new List<Event> {new Event()}};

			target.Consume(message);

			bus.AssertWasCalled(x => x.Send(message.Events.ToArray()));
		}

		[Test]
		public void ShouldPassOnEventsInitiatorId()
		{
            var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
            var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var target = new EventsConsumer(MockRepository.GenerateMock<IEventPublisher>(), null, new FakeCurrentUnitOfWorkFactory(unitOfWorkFactory));
			var @event = new AnEventWithInitiatorId
				{
					InitiatorId = Guid.NewGuid()
				};
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork(Arg<IInitiatorIdentifier>.Is.Anything)).Return(unitOfWork);

			target.Consume(@event);

			unitOfWorkFactory.AssertWasCalled(x => x.CreateAndOpenUnitOfWork(Arg<IInitiatorIdentifier>.Matches(i => i.InitiatorId == @event.InitiatorId)));
		}

	}

	public class AnEventWithInitiatorId : RaptorDomainEvent
	{
	}

	public class FakeCurrentUnitOfWorkFactory : ICurrentUnitOfWorkFactory
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public FakeCurrentUnitOfWorkFactory(IUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public IUnitOfWorkFactory LoggedOnUnitOfWorkFactory()
		{
			return _unitOfWorkFactory;
		}
	}

}
