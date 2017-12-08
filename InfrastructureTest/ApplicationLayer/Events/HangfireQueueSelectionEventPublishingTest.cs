using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	[TestFixture]
	[HangfireTest]
	public class HangfireQueueSelectionEventPublishingTest : ISetup
	{
		public HangfireUtilities Hangfire;
		public IEventPublisher Publisher;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<DefaultQueueHandler>();
			system.AddService<OtherQueueHandler>();
		}

		[Test]
		public void ShouldEnqueueOnDefaultQueue()
		{
			Publisher.Publish(new DefaultQueueEvent());

			Hangfire.NumberOfJobsInQueue(Queues.Default).Should().Be(1);
			Hangfire.NumberOfJobsInQueue(Queues.CriticalScheduleChangesToday).Should().Be(0);
		}

		[Test]
		public void ShouldEnqueueOnOtherQueue()
		{
			Publisher.Publish(new OtherQueueEvent());

			Hangfire.NumberOfJobsInQueue(Queues.Default).Should().Be(0);
			Hangfire.NumberOfJobsInQueue(Queues.CriticalScheduleChangesToday).Should().Be(1);
		}

		public class DefaultQueueEvent : IEvent
		{
		}

		public class DefaultQueueHandler :
			IHandleEvent<DefaultQueueEvent>,
			IHandleEventOnQueue<DefaultQueueEvent>,
			IRunOnHangfire
		{
			public void Handle(DefaultQueueEvent @event)
			{
			}

			public string QueueTo(DefaultQueueEvent @event)
			{
				return null;
			}
		}

		public class OtherQueueEvent : IEvent
		{
		}

		public class OtherQueueHandler :
			IHandleEvent<OtherQueueEvent>,
			IHandleEventOnQueue<OtherQueueEvent>,
			IRunOnHangfire
		{
			public void Handle(OtherQueueEvent @event)
			{
			}

			public string QueueTo(OtherQueueEvent @event)
			{
				return Queues.CriticalScheduleChangesToday;
			}
		}

	}
}