using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	[TestFixture]
	[HangfireTest]
	public class HangfireRetryEventPublishingTest : ISetup
	{
		public Lazy<HangfireUtilities> Hangfire;
		public IEventPublisher Publisher;
		public IRecurringEventPublisher Recurring;
		public HangfireClientStarter Starter;
		public FailingHandlerImpl FailingHandler;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<FailingHandlerImpl>();
		}
		
		[Test]
		public void ShouldScheduleRetry()
		{
			Publisher.Publish(new RetryEvent());

			Hangfire.Value.WorkerIteration();

			Hangfire.Value.NumberOfScheduledJobs().Should().Be(1);
			Hangfire.Value.NumberOfJobsInQueue(Queues.Default).Should().Be(0);
		}

		[Test]
		public void ShouldScheduleSecondAttempt()
		{
			Publisher.Publish(new RetryEvent());

			Hangfire.Value.WorkerIteration();
			Hangfire.Value.RequeueScheduledJobs();
			Hangfire.Value.WorkerIteration();

			Hangfire.Value.NumberOfScheduledJobs().Should().Be(1);
		}

		[Test]
		public void ShouldFailOnLastAttempt()
		{
			Publisher.Publish(new RetryEvent());

			Hangfire.Value.WorkerIteration();
			Hangfire.Value.RequeueScheduledJobs();
			Hangfire.Value.WorkerIteration();
			Hangfire.Value.RequeueScheduledJobs();
			Hangfire.Value.WorkerIteration();

			Hangfire.Value.NumberOfFailedJobs().Should().Be(1);
			Hangfire.Value.NumberOfScheduledJobs().Should().Be(0);
		}

		[Test]
		public void ShouldSucceedOnSecondAttempt()
		{
			Publisher.Publish(new RetryEvent());

			Hangfire.Value.WorkerIteration();
			Hangfire.Value.RequeueScheduledJobs();
			FailingHandler.Succeeds = true;
			Hangfire.Value.WorkerIteration();

			Hangfire.Value.NumberOfFailedJobs().Should().Be(0);
			Hangfire.Value.NumberOfScheduledJobs().Should().Be(0);
		}

		[Test]
		public void ShouldRetry3TimeByDefault()
		{
			Publisher.Publish(new DefaultRetryEvent());

			10.Times(() =>
			{
				Hangfire.Value.WorkerIteration();
				Hangfire.Value.RequeueScheduledJobs();
			});

			FailingHandler.Attempts.Should().Be(3);
		}

		[Test]
		public void ShouldNotRetryMinutelyRecurringJobs()
		{
			Recurring.PublishMinutely(new RecurringEvent());

			Hangfire.Value.TriggerReccuringJobs();
			20.Times(() =>
			{
				Hangfire.Value.WorkerIteration();
				Hangfire.Value.RequeueScheduledJobs();
			});

			FailingHandler.Attempts.Should().Be(1);
		}

		[Test]
		public void ShouldNotRetryHourlyRecurringJobs()
		{
			Recurring.PublishHourly(new RecurringEvent());

			Hangfire.Value.TriggerReccuringJobs();
			20.Times(() =>
			{
				Hangfire.Value.WorkerIteration();
				Hangfire.Value.RequeueScheduledJobs();
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