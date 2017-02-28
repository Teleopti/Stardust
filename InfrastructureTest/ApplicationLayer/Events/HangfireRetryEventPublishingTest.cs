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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	[TestFixture]
	[HangfireTest]
	public class HangfireRetryEventPublishingTest : ISetup
	{
		public HangfireUtilities Hangfire;
		public IEventPublisher Publisher;
		public IRecurringEventPublisher Recurring;
		public FailingHandlerImpl FailingHandler;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<FailingHandlerImpl>();
		}
		
		[Test]
		public void ShouldScheduleRetry()
		{
			Publisher.Publish(new RetryEvent());

			Hangfire.WorkerIteration();

			Hangfire.NumberOfScheduledJobs().Should().Be(1);
			Hangfire.NumberOfJobsInQueue(Queues.Default).Should().Be(0);
		}

		[Test]
		public void ShouldScheduleSecondAttempt()
		{
			Publisher.Publish(new RetryEvent());

			Hangfire.WorkerIteration();
			Hangfire.RequeueScheduledJobs();
			Hangfire.WorkerIteration();

			Hangfire.NumberOfScheduledJobs().Should().Be(1);
		}

		[Test]
		public void ShouldFailOnLastAttempt()
		{
			Publisher.Publish(new RetryEvent());

			Hangfire.WorkerIteration();
			Hangfire.RequeueScheduledJobs();
			Hangfire.WorkerIteration();
			Hangfire.RequeueScheduledJobs();
			Hangfire.WorkerIteration();

			Hangfire.NumberOfFailedJobs().Should().Be(1);
			Hangfire.NumberOfScheduledJobs().Should().Be(0);
		}

		[Test]
		public void ShouldSucceedOnSecondAttempt()
		{
			Publisher.Publish(new RetryEvent());

			Hangfire.WorkerIteration();
			Hangfire.RequeueScheduledJobs();
			FailingHandler.Succeeds = true;
			Hangfire.WorkerIteration();

			Hangfire.NumberOfFailedJobs().Should().Be(0);
			Hangfire.NumberOfScheduledJobs().Should().Be(0);
		}

		[Test]
		public void ShouldRetry3TimesByDefault()
		{
			Publisher.Publish(new DefaultRetryEvent());

			10.Times(() =>
			{
				Hangfire.WorkerIteration();
				Hangfire.RequeueScheduledJobs();
			});

			FailingHandler.Attempts.Should().Be(3);
		}

		[Test]
		public void ShouldNotRetryMinutelyRecurringJobs()
		{
			Recurring.PublishMinutely(new RecurringEvent());

			Hangfire.TriggerReccuringJobs();
			20.Times(() =>
			{
				Hangfire.WorkerIteration();
				Hangfire.RequeueScheduledJobs();
			});

			FailingHandler.Attempts.Should().Be(1);
		}
		
		public class RetryEvent : IEvent
		{
		}

		public class DefaultRetryEvent : IEvent
		{
		}

		public class RecurringEvent : IEvent
		{
		}

		public class FailingHandlerImpl :
			IHandleEvent<RetryEvent>,
			IHandleEvent<DefaultRetryEvent>,
			IHandleEvent<RecurringEvent>,
			IRunOnHangfire
		{

			public int Attempts = 0;
			public bool Succeeds = false;

			[Attempts(3)]
			public void Handle(RetryEvent @event)
			{
				Attempts++;
				if (!Succeeds)
					throw new Exception("fail!");
			}

			public void Handle(DefaultRetryEvent @event)
			{
				Attempts++;
				if (!Succeeds)
					throw new Exception("fail!");
			}

			public void Handle(RecurringEvent @event)
			{
				Attempts++;
				if (!Succeeds)
					throw new Exception("fail!");
			}

		}
	}
}