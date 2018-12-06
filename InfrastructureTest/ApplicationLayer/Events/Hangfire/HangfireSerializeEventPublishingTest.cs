using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire
{
	[TestFixture]
	[HangfireTest]
	public class HangfireSerializeEventPublishingTest : IExtendSystem
	{
		public Lazy<HangfireUtilities> Hangfire;
		public IEventPublisher Publisher;
		public IRecurringEventPublisher Recurring;
		public TestHandler Handler;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<TestHandler>();
		}

		[Test]
		public void ShouldProcess()
		{
			Publisher.Publish(new TestEvent());

			Hangfire.Value.EmulateWorkerIteration();

			Handler.GotEvent.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReceiveEventData()
		{
			Publisher.Publish(new TestEvent {Data = "hello"});

			Hangfire.Value.EmulateWorkerIteration();

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