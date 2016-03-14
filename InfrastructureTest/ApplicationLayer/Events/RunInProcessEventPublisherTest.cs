using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	[InfrastructureTest]
	public class RunInProcessEventPublisherTest : ISetup
	{
		public RunInProcessEventPublisher Target;

		[Test]
		public void ShouldRunEventHandlerImmediatly()
		{
			var @event = new TestEvent();
			Target.Publish(@event);

			@event.Handler.WasCalled.Should().Be.SameInstanceAs(@event);
		}

		[Test]
		public void ShouldCreateNewScope()
		{
			var event1 = new TestEvent();
			var event2 = new TestEvent();
			Target.Publish(event1, event2);
			event1.Handler.Should().Not.Be.SameInstanceAs(event2.Handler);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<TestEventHandler>(true);
		}
	}

	public class TestEventHandler : IHandleEvent<TestEvent>, IRunInProcess
	{
		public TestEvent WasCalled;

		public void Handle(TestEvent @event)
		{
			WasCalled = @event;
			@event.Handler = this;
		}
	}

	public class TestEvent : IEvent
	{
		public TestEventHandler Handler { get; set; }
	}
}