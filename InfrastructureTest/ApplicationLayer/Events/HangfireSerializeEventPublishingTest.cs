using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	[TestFixture]
	[HangfireTest]
	public class HangfireSerializeEventPublishingTest : ISetup
	{
		public Lazy<HangfireUtilities> Hangfire;
		public IEventPublisher Publisher;
		public IRecurringEventPublisher Recurring;
		public HangfireClientStarter Starter;
		public TestHandler Handler;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<TestHandler>();
		}

		[Test]
		public void ShouldProcess()
		{
			Publisher.Publish(new TestEvent());

			Hangfire.Value.WorkerIteration();

			Handler.GotEvent.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReceiveEventData()
		{
			Publisher.Publish(new TestEvent {Data = "hello"});

			Hangfire.Value.WorkerIteration();

			Handler.GotEvent.Data.Should().Be("hello");
		}

		public class TestEvent : IEvent
		{
			public string Data;
		}
		
		public class TestHandler :
			IHandleEvent<TestEvent>,
			IRunOnHangfire
		{

			public TestEvent GotEvent;

			public void Handle(TestEvent @event)
			{
				GotEvent = @event;
			}

		}
	}
}