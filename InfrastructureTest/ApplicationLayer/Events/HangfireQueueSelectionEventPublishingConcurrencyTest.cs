using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	[TestFixture]
	[HangfireTest]
	public class HangfireQueueSelectionEventPublishingConcurrencyTest : ISetup
	{
		public HangfireUtilities Hangfire;
		public IEventPublisher Publisher;
		public ConcurrencyRunner Run;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<QueuingHandler>();
		}

		[Test]
		public void ShouldEnqueueToDefinedQueuesConcurrently()
		{
			1000.Times(() =>
			{
				Run.InParallel(() =>
				{
					Publisher.Publish(new DefaultQueueEvent());
				});
				Run.InParallel(() =>
				{
					Publisher.Publish(new OtherQueueEvent());
				});
			});
			Run.Wait();

			Hangfire.NumberOfJobsInQueue(Queues.Default).Should().Be(1000);
			Hangfire.NumberOfJobsInQueue(Queues.CriticalScheduleChangesToday).Should().Be(1000);
		}
		
		public class DefaultQueueEvent : IEvent
		{
		}

		public class OtherQueueEvent : IEvent
		{
		}

		public class QueuingHandler :
			IHandleEvent<DefaultQueueEvent>,
			IHandleEventOnQueue<DefaultQueueEvent>,
			IHandleEvent<OtherQueueEvent>,
			IHandleEventOnQueue<OtherQueueEvent>,
			IRunOnHangfire
		{
			public void Handle(DefaultQueueEvent @event)
			{
			}

			public string QueueTo(DefaultQueueEvent @event)
			{
				return null;
			}

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