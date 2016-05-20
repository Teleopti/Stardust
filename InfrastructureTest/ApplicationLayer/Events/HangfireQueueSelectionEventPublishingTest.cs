using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	[TestFixture]
	[AnalyticsDatabaseTest]
	public class HangfireQueueSelectionEventPublishingTest : ISetup
	{
		public IHangfireUtilities Hangfire;
		public IEventPublisher Publisher;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<HangfireEventClient>();// register over the fake, which already registeres over the common registration ;)
			system.AddService<NormalPriorityHandler>();
			system.AddService<HighPriorityHandler>();
		}

		[Test]
		public void ShouldEnqueueOnDefaultQueue()
		{
			var before = Hangfire.NumberOfJobsInQueue(QueueName.Default);
			Publisher.Publish(new NormalPriorityEvent());

			Hangfire.NumberOfJobsInQueue(QueueName.Default).Should().Be(before+1);
			//Hangfire.NumberOfJobsInQueue(QueueName.HighPriority).Should().Be(0);
		}

		//[Test]
		//public void ShouldEnqueueOnHighPriority()
		//{
		//	Publisher.Publish(new HighPriorityEvent());

		//	Hangfire.NumberOfJobsInQueue(QueueName.Default).Should().Be(0);
		//	Hangfire.NumberOfJobsInQueue(QueueName.HighPriority).Should().Be(1);
		//}

		public class HighPriorityEvent : IEvent
		{
		}

		public class HighPriorityHandler : 
			IHandleEvent<HighPriorityEvent>, 
			IRunOnHangfire//,
			//IRunWithHighPriority
		{
			public void Handle(HighPriorityEvent @event)
			{
			}
		}

		public class NormalPriorityEvent : IEvent
		{
		}

		public class NormalPriorityHandler :
			IHandleEvent<NormalPriorityEvent>,
			IRunOnHangfire//,
			//IRunWithDefaultPriority
		{
			public void Handle(NormalPriorityEvent @event)
			{
			}
		}
	}
}