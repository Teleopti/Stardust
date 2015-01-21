using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer
{
	[TestFixture]
	public class HangfireOrBusEventPublisherTest
	{
		[Test]
		public void ShouldPublishHangfireEventsToHangfire()
		{
			var hangfire = new FakeHangfireEventClient();
			var target = new HangfireOrBusEventPublisher(new HangfireEventPublisher(hangfire, new NewtonsoftJsonSerializer(), null), null);

			target.Publish(new HangfireEvent());

			hangfire.WasEnqueued.Should().Be.True();
		}

		[Test]
		public void ShouldPublishNonHangfireEventsToTheBus()
		{
			var bus = new FakeServiceBusSender();
			var target = new HangfireOrBusEventPublisher(null, new ServiceBusEventPublisher(bus));
			var @event = new Event();

			target.Publish(@event);

			bus.SentMessages.Single().Should().Be(@event);
		}

		[Test]
		public void ShouldPublishRtaEventsToHangfire()
		{
			var hangfire = new FakeHangfireEventClient();
			var target = new HangfireOrBusEventPublisher(new HangfireEventPublisher(hangfire, new NewtonsoftJsonSerializer(), null), null);

			target.Publish(new PersonShiftStartEvent());
			target.Publish(new PersonShiftEndEvent());
			target.Publish(new PersonActivityStartEvent());
			target.Publish(new PersonStateChangedEvent());
			target.Publish(new PersonInAdherenceEvent());
			target.Publish(new PersonOutOfAdherenceEvent());

			hangfire.Events.Should().Have.Count.EqualTo(6);
		}
	}

	public class HangfireEvent : IEvent, IGoToHangfire
	{
	}
}