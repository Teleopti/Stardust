using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	[TestFixture]
	[InfrastructureTest]
	public class MultiEventPublishingTest : IIsolateSystem, IExtendSystem
	{
		public FakeHangfireEventClient Hangfire;
		public FakeStardustSender Stardust;
		public IEventPublisher Target;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<HangfireEventHandler>();
			extend.AddService<StardustEventHandler>();
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeStardustSender>().For<IStardustSender>();
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

			Stardust.SentMessages.Single().Should().Be.SameInstanceAs(@event);
		}

		[Test]
		public void ShouldPublishEventsWithHangfireAndBusHandlerToBoth()
		{
			Target.Publish(new EventWithBothHandlers());

			Hangfire.WasEnqueued.Should().Be.True();
			Stardust.SentMessages.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldNotPublishEventsWithoutHandler()
		{
			Target.Publish(new EventWithoutHandler());

			Hangfire.WasEnqueued.Should().Be.False();
			Stardust.SentMessages.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldNotPublishEventsWithHangfireHandlerToTheBus()
		{
			Target.Publish(new EventWithHangfireHandler());

			Stardust.SentMessages.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldNotPublishEventsWithBothHandlersToBusHandlerOnHangfire()
		{
			Target.Publish(new EventWithBothHandlers());

			Hangfire.HandlerTypeNames.Any(x => x.Contains(typeof (StardustEventHandler).Name)).Should().Be.False();
			Stardust.SentMessages.Should().Have.Count.EqualTo(1);
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

		public class StardustEventHandler :
			IRunOnStardust,
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