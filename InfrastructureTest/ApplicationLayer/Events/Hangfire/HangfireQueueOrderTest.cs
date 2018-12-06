using System;
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

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire
{
	[TestFixture]
	[HangfireTest]
	public class HangfireQueueOrderTest :IExtendSystem	
	{
		public HangfireUtilities Hangfire;
		public IEventPublisher Publisher;
		public QueueScheduleChangesTodayHandler QueueScheduleChangesTodayHandlerImpl;
		public QueueDefaultHandler QueueDefaultHandlerImpl;
		public QueueCriticalScheduleChangesTodayHandler QueueCriticalScheduleChangesTodayHandlerImpl;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<QueueScheduleChangesTodayHandler>();
			extend.AddService<QueueCriticalScheduleChangesTodayHandler>();
			extend.AddService<QueueDefaultHandler>();
		}

		[Test]
		public void ShouldProcessQueuesInAlphaBeticalOrder()
		{
			Publisher.Publish(new QueueScheduleChangesTodayEvent());
			Publisher.Publish(new QueueCriticalScheduleChangesTodayEvent());
			Publisher.Publish(new QueueDefaultEvent());

			Hangfire.EmulateQueueProcessing(1);

			Hangfire.WaitForQueue();

			QueueCriticalScheduleChangesTodayHandlerImpl.HandledTimeStamp.Ticks.Should().Be.LessThan(QueueDefaultHandlerImpl.HandledTimeStamp.Ticks);
			QueueDefaultHandlerImpl.HandledTimeStamp.Ticks.Should().Be.LessThan(QueueScheduleChangesTodayHandlerImpl.HandledTimeStamp.Ticks);
		}

		public class QueueScheduleChangesTodayEvent : IEvent
		{
		}

		public class QueueDefaultEvent : IEvent
		{
		}

		public class QueueCriticalScheduleChangesTodayEvent : IEvent
		{
		}

		public class QueueScheduleChangesTodayHandler :
			IHandleEvent<QueueScheduleChangesTodayEvent>,
			IHandleEventOnQueue<QueueScheduleChangesTodayEvent>,
			IRunOnHangfire
		{
			public DateTime HandledTimeStamp;

			public void Handle(QueueScheduleChangesTodayEvent @event)
			{
				HandledTimeStamp = DateTime.UtcNow;
			}

			public string QueueTo(QueueScheduleChangesTodayEvent @event)
			{
#pragma warning disable 618
				return Queues.ScheduleChangesToday;
#pragma warning restore 618
			}
		}

		public class QueueDefaultHandler :
			IHandleEvent<QueueDefaultEvent>,
			IHandleEventOnQueue<QueueDefaultEvent>,
			IRunOnHangfire
		{
			public DateTime HandledTimeStamp;

			public void Handle(QueueDefaultEvent @event)
			{
				HandledTimeStamp = DateTime.UtcNow;
			}

			public string QueueTo(QueueDefaultEvent @event)
			{
				return null;
			}
		}

		public class QueueCriticalScheduleChangesTodayHandler :
			IHandleEvent<QueueCriticalScheduleChangesTodayEvent>,
			IHandleEventOnQueue<QueueCriticalScheduleChangesTodayEvent>,
			IRunOnHangfire
		{
			public DateTime HandledTimeStamp;

			public void Handle(QueueCriticalScheduleChangesTodayEvent @event)
			{
				HandledTimeStamp = DateTime.UtcNow;
			}

			public string QueueTo(QueueCriticalScheduleChangesTodayEvent @event)
			{
				return Queues.CriticalScheduleChangesToday;
			}
		}
	}
}