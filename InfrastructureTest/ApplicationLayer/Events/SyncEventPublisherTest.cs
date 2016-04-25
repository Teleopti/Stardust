﻿using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	[TestFixture]
	[InfrastructureTest]
	[Setting("BehaviorTestServer", true)]
	public class SyncEventPublisherTest : ISetup
	{
		public FakeHandler Handler;
		public FakeHandler1 Handler1;
		public FakeHandler2 Handler2;
		public IEventPublisher Target;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<FakeHandler>();
			system.AddService<FakeHandler1>();
			system.AddService<FakeHandler2>();
		}

		[Test]
		public void ShouldInvokeHandler()
		{
			var @event = new TestEvent();

			Target.Publish(@event);

			Handler.CalledWithEvent.Should().Be(@event);
		}

		[Test]
		public void ShouldInvokeMultipleHandlers()
		{
			var @event = new TestEvent();

			Target.Publish(@event);

			Handler1.CalledWithEvent.Should().Be(@event);
			Handler2.CalledWithEvent.Should().Be(@event);
		}

		[Test]
		public void ShouldCallCorrectHandleMethod()
		{
			var @event = new TestEventTwo();

			Target.Publish(@event);

			Handler.CalledWithEventTwo.Should().Be(@event);
		}

		public class TestEvent : Event
		{
		}

		public class TestEventTwo : Event
		{
		}

		public class TestDomainEvent : EventWithInfrastructureContext
		{
		}
		
		public class FakeHandler1 : FakeHandler
		{
			
		}

		public class FakeHandler2 : FakeHandler
		{
			
		}

#pragma warning disable 618
		public class FakeHandler : 
			IHandleEvent<TestEvent>, 
			IHandleEvent<TestEventTwo>, 
			IHandleEvent<TestDomainEvent>,
			IRunOnServiceBus
#pragma warning restore 618
		{
			public Event CalledWithEvent;
			public Event CalledWithEventTwo;

			public void Handle(TestEvent @event)
			{
				CalledWithEvent = @event;
			}

			public void Handle(TestEventTwo @event)
			{
				CalledWithEventTwo = @event;
			}

			public void Handle(TestDomainEvent @event)
			{
			}
		}
	}
}
