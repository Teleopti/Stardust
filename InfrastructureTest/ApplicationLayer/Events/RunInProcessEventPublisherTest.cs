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
		public TestEventHandler TestEventHandler;

		[Test, Ignore("Fix this Monday")]
		public void ShouldRunEventHandlerImmediatly()
		{
			var @event = new TestEvent();
			Target.Publish(@event);

			TestEventHandler.WasCalled.Should().Be.SameInstanceAs(@event);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<TestEventHandler>();
		}
	}

	public class TestEventHandler : IHandleEvent<TestEvent>, IRunInProcess
	{
		public TestEvent WasCalled;

		public void Handle(TestEvent @event)
		{
			WasCalled = @event;
		}
	}

	public class TestEvent : IEvent
	{
	}
}