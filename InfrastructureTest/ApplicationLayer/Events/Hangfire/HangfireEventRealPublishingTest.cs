using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire
{
	[TestFixture]
	[RealHangfireTest]
	public class HangfireEventRealPublishingTest : IExtendSystem, ITestInterceptor
	{
		public HangfireUtilities Hangfire;
		public IEventPublisher Publisher;
		public TestHandler Handler;
		public PersistedTypeMapperForTest TypeMapper;

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<TestHandler>();
		}

		public void OnBefore()
		{
			TypeMapper.DynamicMappingsForTestProjects = false;
		}

		[Test]
		public void ShouldProcess()
		{
			Publisher.Publish(new TestEvent());

			Hangfire.EmulateWorkerIteration();

			Handler.Received.Should().Be.OfType<TestEvent>();
		}

		public class TestEvent : IEvent
		{
		}

		public class TestHandler :
			IHandleEvent<TestEvent>,
			IRunOnHangfire
		{
			public IEvent Received;

			public void Handle(TestEvent @event)
			{
				Received = @event;
			}
		}

	}
}