using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	[TestFixture]
	[InfrastructureTest]
	[Toggle(Toggles.RTA_NewEventHangfireRTA_34333)]
	public class SelectiveEventPublishingTest : ISetup
	{
		public FakeHangfireEventClient Hangfire;
		public FakeServiceBusSender Bus;
		public IEventPublisher Target;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeHangfireEventClient>().For<IHangfireEventClient>();
			system.UseTestDouble<FakeServiceBusSender>().For<IServiceBusSender>();
			
			system.AddService<HangfireEventHandler>();
			system.AddService<BusEventHandler>();
		}

		[Test]
		public void ShouldPublishEventsWithHangfireHandlerToHangfire()
		{
			Target.Publish(new EventWithHangfireHandler());

			Hangfire.WasEnqueued.Should().Be.True();
		}

		[Test]
		public void ShouldPublishEventsWithBusHandlerToTheBus()
		{
			var @event = new EventWithBusHandler();

			Target.Publish(@event);

			Bus.SentMessages.Single().Should().Be.SameInstanceAs(@event);
		}

		[Test]
		public void ShouldPublishEventsWithHangfireAndBusHandlerToBoth()
		{
			Target.Publish(new EventWithBothHandlers());

			Hangfire.WasEnqueued.Should().Be.True();
			Bus.SentMessages.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldNotPublishEventsWithoutHandler()
		{
			Target.Publish(new EventWithoutHandler());

			Hangfire.WasEnqueued.Should().Be.False();
			Bus.SentMessages.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldNotPublishEventsWithHangfireHandlerToTheBus()
		{
			Target.Publish(new EventWithHangfireHandler());

			Bus.SentMessages.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldNotPublishEventsWithBothHandlersToBusHandlerOnHangfire()
		{
			Target.Publish(new EventWithBothHandlers());

			Hangfire.HandlerTypes.Any(x => x.Contains(typeof (BusEventHandler).Name)).Should().Be.False();
			Bus.SentMessages.Should().Have.Count.EqualTo(1);
		}

		public class EventWithHangfireHandler : IEvent
		{
		}

		public class EventWithBusHandler : IEvent
		{
		}

		public class EventWithBothHandlers : IEvent
		{
		}

		public class EventWithoutHandler : IEvent
		{
		}

		public class HangfireEventHandler :
			IRunOnHangfire,
			IHandleEvent<EventWithHangfireHandler>,
			IHandleEvent<EventWithBothHandlers>
		{
			public void Handle(EventWithHangfireHandler @event)
			{
			}

			public void Handle(EventWithBothHandlers @event)
			{
			}
		}

		public class BusEventHandler :
			IRunOnServiceBus,
			IHandleEvent<EventWithBusHandler>,
			IHandleEvent<EventWithBothHandlers>
		{
			public void Handle(EventWithBusHandler @event)
			{
			}

			public void Handle(EventWithBothHandlers @event)
			{
			}
		}



	}
}