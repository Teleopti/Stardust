using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	[TestFixture]
	[HangfireTest]
	public class HangfireEventPackagePublishingTest : ISetup
	{
		public HangfireUtilities Hangfire;
		public IEventPublisher Publisher;
		public TestHandler Handler;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<TestHandler>();
		}

		[Test]
		public void ShouldProcess()
		{
			Publisher.Publish(new TestEvent());

			Hangfire.EmulateWorkerIteration();

			Handler.Packeged.Single().Should().Be.OfType<TestEvent>();
		}

		[Test]
		public void ShouldProcessTwo()
		{
			Publisher.Publish(new TestEvent(), new TestEvent());

			Hangfire.EmulateWorkerIteration();

			Handler.Packeged.OfType<TestEvent>().Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldReceieveSubscribedEvents()
		{
			Publisher.Publish(new TestEvent(), new AnotherTestEvent());

			Hangfire.EmulateQueueProcessing();

			Handler.Packeged.OfType<TestEvent>().Should().Have.Count.EqualTo(1);
			Handler.Packeged.OfType<AnotherTestEvent>().Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldReceiveAnotherEventSeperately()
		{
			Publisher.Publish(new TestEvent(), new AnotherTestEvent());

			Hangfire.EmulateWorkerIteration();
			Hangfire.EmulateWorkerIteration();

			Handler.Packeged.Should().Have.Count.EqualTo(1);
			Handler.AnotherEvent.Should().Be.OfType<AnotherTestEvent>();
		}
		
		[Test]
		public void ShouldPublishPackagesInBatchesOf50()
		{
			var fiftyOneEvents = 
				Enumerable.Range(1, 51)
				.Select(x => new TestEvent())
				.ToArray();

			Publisher.Publish(fiftyOneEvents);

			Hangfire.EmulateWorkerIteration();
			Handler.Packeged.Should().Have.Count.EqualTo(50);

			Hangfire.EmulateWorkerIteration();
			Handler.Packeged.Should().Have.Count.EqualTo(1);
		}

		public class TestEvent : IEvent
		{
		}

		public class AnotherTestEvent : IEvent
		{
		}

		public class TestHandler :
			IHandleEvents,
			IHandleEvent<AnotherTestEvent>,
			IRunOnHangfire
		{

			public IEnumerable<IEvent> Packeged;
			public IEvent AnotherEvent;

			public void Subscribe(ISubscriptionsRegistrator subscriptions)
			{
				subscriptions.Add<TestEvent>();
			}

			public void Handle(IEnumerable<IEvent> events)
			{
				Packeged = events;
			}

			public void Handle(AnotherTestEvent @event)
			{
				AnotherEvent = @event;
			}
		}
	}
}