using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.ServiceBus.ApplicationLayer;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.ApplicationLayer
{
	[TestFixture]
	public class EventsConsumerTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldPublishEvents()
		{
			var publisher = MockRepository.GenerateMock<IEventPublisher>();
			var target = new EventsConsumer(publisher, null);
			var @event = new Event();

			target.Consume(@event);

			publisher.AssertWasCalled(x => x.Publish(@event));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldSendEventsFromPackageMessage()
		{
			var bus = MockRepository.GenerateMock<IServiceBus>();
			var target = new EventsConsumer(null, bus);
			var message = new EventsPackageMessage {Events = new[] {new Event()}};

			target.Consume(message);

			bus.AssertWasCalled(x => x.Send(message.Events));
		}

	}
}
