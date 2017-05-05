using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	[TestFixture]
	[InfrastructureTest]
	public class SyncEventPackagePublishingTest : ISetup
	{
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

			Handler.Packeged.Single().Should().Be.OfType<TestEvent>();
		}

		[Test]
		public void ShouldProcessTwo()
		{
			Publisher.Publish(new TestEvent(), new TestEvent());

			Handler.Packeged.OfType<TestEvent>().Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldReceieveSubscribedEvents()
		{
			Publisher.Publish(new TestEvent(), new AnotherTestEvent());

			Handler.Packeged.OfType<TestEvent>().Should().Have.Count.EqualTo(1);
			Handler.Packeged.OfType<AnotherTestEvent>().Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldReceiveAnotherEventSeperately()
		{
			Publisher.Publish(new TestEvent(), new AnotherTestEvent());

			Handler.Packeged.Should().Have.Count.EqualTo(1);
			Handler.AnotherEvent.Should().Be.OfType<AnotherTestEvent>();
		}

		[Test]
		public void ShouldRetryEvents()
		{
			Handler.Fails();

			Publisher.Publish(new TestEvent(), new TestEvent());

			Handler.Attempts.Should().Be(2);
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
			IRunInSync
		{

			public IEnumerable<IEvent> Packeged;
			public IEvent AnotherEvent;
			private bool _fails;
			public int Attempts;

			public void Subscribe(ISubscriptionRegistrator registrator)
			{
				registrator.SubscribeTo<TestEvent>();
			}

			[Attempts(2)]
			public void Handle(IEnumerable<IEvent> events)
			{
				Attempts += 1;

				if (_fails)
					throw new Exception();

				Packeged = events;
			}

			public void Handle(AnotherTestEvent @event)
			{
				AnotherEvent = @event;
			}

			public void Fails()
			{
				_fails = true;
			}
		}
	}
}