using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.ServiceBus.ApplicationLayer;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

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
			var target = new EventsConsumer(publisher, null, currentUnitOfWorkFactory, null);
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
			var target = new EventsConsumer(MockRepository.GenerateMock<IEventPublisher>(), null, currentUnitOfWorkFactory, null);

			target.Consume(new Event());

			unitOfWork.AssertWasCalled(x => x.PersistAll());
		}

		[Test]
		public void ShouldSendEventsFromPackageMessage()
		{
			var bus = MockRepository.GenerateMock<IServiceBus>();
			var target = new EventsConsumer(null, bus, null, null);
			var message = new EventsPackageMessage {Events = new List<Event> {new Event()}};

			target.Consume(message);

			bus.AssertWasCalled(x => x.Send(message.Events.ToArray()));
		}

		[Test]
		public void ShouldPassOnEventsInitiatorId()
		{
            var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
            var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var target = new EventsConsumer(MockRepository.GenerateMock<IEventPublisher>(), null, new FakeCurrentUnitOfWorkFactory(unitOfWorkFactory), null);
			var @event = new AnEventWithInitiatorId
				{
					InitiatorId = Guid.NewGuid()
				};
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork(Arg<IInitiatorIdentifier>.Is.Anything)).Return(unitOfWork);

			target.Consume(@event);

			unitOfWorkFactory.AssertWasCalled(x => x.CreateAndOpenUnitOfWork(Arg<IInitiatorIdentifier>.Matches(i => i.InitiatorId == @event.InitiatorId)));
		}

		[Test]
		public void ShouldSendTrackingMessageIfExceptionThrown()
		{
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			currentUnitOfWorkFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(unitOfWorkFactory);
			var publisher = MockRepository.GenerateMock<IEventPublisher>();
			var trackingMessageSender = MockRepository.GenerateMock<ITrackingMessageSender>();
			var target = new EventsConsumer(publisher, null, currentUnitOfWorkFactory, trackingMessageSender);
			var initiatorId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var @event = new ActivityAddedEvent
			{
				InitiatorId = initiatorId,
				BusinessUnitId = businessUnitId,
				TrackId = trackId
			};
			publisher.Stub(x => x.Publish(@event)).Throw(new Exception());


			try
			{
				target.Consume(@event);
			}
			catch (Exception)
			{
				
			}

			var args =
				trackingMessageSender.GetArgumentsForCallsMadeOn(
					x => x.SendTrackingMessage(initiatorId, businessUnitId, new TrackingMessage()), x => x.IgnoreArguments());
			var firstCall = args.Single();
			firstCall.First().Should().Be(initiatorId);
			firstCall.ElementAt(1).Should().Be(businessUnitId);
			(firstCall.ElementAt(2) as TrackingMessage).TrackId.Should().Be(trackId);
			(firstCall.ElementAt(2) as TrackingMessage).Status.Should().Be(TrackingMessageStatus.Failed);

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
