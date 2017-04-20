using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	[InfrastructureTest]
	public class RunInSyncInFatClientProcessEventPublisherTest : ISetup
	{
		public RunInSyncInFatClientProcessEventPublisher Target;
		public TestEventHandler Handler;
		public TestEventHandler2 Handler2;
		public IncorrectTestEventHandler IncorrectHandler;

		[Test]
		public void ShouldCreateNewScope()
		{
			var event1 = new TestEvent();
			var event2 = new TestEvent();
			Target.Publish(event1, event2);

			var testEventHandler1 = event1.HandleByEventHandlers().Single(x => x is TestEventHandler);
			var testEventHandler2 = event2.HandleByEventHandlers().Single(x => x is TestEventHandler);
			testEventHandler1.Should().Not.Be.SameInstanceAs(testEventHandler2);
		}

		[Test]
		public void ShouldSupportMultipleHandlersForOneTypeOfEvent()
		{
			var @event = new TestEvent();

			Target.Publish(@event);

			@event.HandleByEventHandlers().Count()
				.Should().Be.GreaterThan(1);
		}

		[Test]
		public void ShouldOnlyCareAboutRunInProcessEvents()
		{
			var @event = new TestEvent();

			Target.Publish(@event);

			@event.HandleByEventHandlers().Where(x => x is IncorrectTestEventHandler)
				.Should().Be.Empty();
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<TestEventHandler>(true);
			system.AddService<TestEventHandler2>(true);
			system.AddService<IncorrectTestEventHandler>(true);
		}


		public class TestEventHandler : IHandleEvent<TestEvent>, IRunInSyncInFatClientProcess
		{
			public void Handle(TestEvent @event)
			{
				@event.WasHandledBy(this);
			}
		}

		public class TestEventHandler2 : IHandleEvent<TestEvent>, IRunInSyncInFatClientProcess
		{
			public void Handle(TestEvent @event)
			{
				@event.WasHandledBy(this);
			}
		}

#pragma warning disable 618
		public class IncorrectTestEventHandler : IHandleEvent<TestEvent>, IRunOnHangfire, IRunOnServiceBus, IRunOnStardust
#pragma warning restore 618
		{
			public void Handle(TestEvent @event)
			{
				@event.WasHandledBy(this);
			}
		}

		public class TestEvent : IEvent
		{
			private readonly ConcurrentBag<IHandleEvent<TestEvent>> handledBy = new ConcurrentBag<IHandleEvent<TestEvent>>();

			public void WasHandledBy(IHandleEvent<TestEvent> eventHandler)
			{
				handledBy.Add(eventHandler);
			}

			public IEnumerable<IHandleEvent<TestEvent>> HandleByEventHandlers()
			{
				return handledBy;
			}
		}
	}

}