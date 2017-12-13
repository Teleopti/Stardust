﻿using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
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
			system.AddService<QueuingHandler>();
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
			Publisher.Publish(new CriticalScheduleChangesTodayQueueEvent());

			Hangfire.NumberOfJobsInQueue(Queues.Default).Should().Be(0);
			Hangfire.NumberOfJobsInQueue(Queues.CriticalScheduleChangesToday).Should().Be(1);
		}
	}
}