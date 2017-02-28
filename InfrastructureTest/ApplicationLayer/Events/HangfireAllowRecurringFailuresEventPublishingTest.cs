﻿using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	[TestFixture]
	[HangfireTest]
	public class HangfireAllowRecurringFailuresEventPublishingTest : ISetup
	{
		public HangfireUtilities Hangfire;
		public IEventPublisher Publisher;
		public IRecurringEventPublisher Recurring;
		public HangfireClientStarter Starter;
		public FailingHandlerImpl FailingHandler;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<FailingHandlerImpl>();
			system.AddService<FailingHandlerImpl2>();
		}

		[Test]
		public void ShouldSucceedRecurringFailures()
		{
			Recurring.PublishMinutely(new RecurringEvent());

			Hangfire.TriggerReccuringJobs();
			Hangfire.WorkerIteration();

			Hangfire.NumberOfFailedJobs().Should().Be(0);
			Hangfire.NumberOfSucceededJobs().Should().Be(1);
		}

		[Test]
		public void ShouldFailAfterLastAllowedFailure()
		{
			Recurring.PublishMinutely(new RecurringEvent());

			Hangfire.TriggerReccuringJobs();
			Hangfire.WorkerIteration();
			Hangfire.TriggerReccuringJobs();
			Hangfire.WorkerIteration();
			Hangfire.TriggerReccuringJobs();
			Hangfire.WorkerIteration();

			Hangfire.NumberOfFailedJobs().Should().Be(1);
			Hangfire.NumberOfSucceededJobs().Should().Be(2);
		}

		[Test]
		public void ShouldCountFailuresForEachRecurringJob()
		{
			Recurring.PublishMinutely(new RecurringEvent());
			Recurring.PublishMinutely(new RecurringEvent2());

			Hangfire.TriggerReccuringJobs();
			Hangfire.WorkerIteration();
			Hangfire.WorkerIteration();
			Hangfire.TriggerReccuringJobs();
			Hangfire.WorkerIteration();
			Hangfire.WorkerIteration();
			Hangfire.TriggerReccuringJobs();
			Hangfire.WorkerIteration();
			Hangfire.WorkerIteration();

			Hangfire.NumberOfFailedJobs().Should().Be(2);
			Hangfire.NumberOfSucceededJobs().Should().Be(4);
		}


		[Test]
		public void ShouldAllowFailuresAfterSuccess()
		{
			Recurring.PublishMinutely(new RecurringEvent());
			FailingHandler.Success = false;
			Hangfire.TriggerReccuringJobs();
			Hangfire.WorkerIteration();
			FailingHandler.Success = true;
			Hangfire.TriggerReccuringJobs();
			Hangfire.WorkerIteration();

			FailingHandler.Success = false;
			Hangfire.TriggerReccuringJobs();
			Hangfire.WorkerIteration();
			Hangfire.TriggerReccuringJobs();
			Hangfire.WorkerIteration();
			Hangfire.TriggerReccuringJobs();
			Hangfire.WorkerIteration();

			Hangfire.NumberOfFailedJobs().Should().Be(1);
		}

		[Test]
		public void ShouldContinueFailingAfterLastAllowedFailure()
		{
			Recurring.PublishMinutely(new RecurringEvent());

			Hangfire.TriggerReccuringJobs();
			Hangfire.WorkerIteration();
			Hangfire.TriggerReccuringJobs();
			Hangfire.WorkerIteration();
			Hangfire.TriggerReccuringJobs();
			Hangfire.WorkerIteration();
			Hangfire.TriggerReccuringJobs();
			Hangfire.WorkerIteration();

			Hangfire.NumberOfFailedJobs().Should().Be(2);
			Hangfire.NumberOfSucceededJobs().Should().Be(2);
		}

		public class RecurringEvent : IEvent
		{
		}

		public class FailingHandlerImpl :
			IHandleEvent<RecurringEvent>,
			IRunOnHangfire
		{
			public bool Success = false;

			[AllowFailures(2)]
			public void Handle(RecurringEvent @event)
			{
				if (!Success)
					throw new Exception("fail!");
			}

		}

		public class RecurringEvent2 : IEvent
		{
		}

		public class FailingHandlerImpl2 :
			IHandleEvent<RecurringEvent2>,
			IRunOnHangfire
		{

			[AllowFailures(2)]
			public void Handle(RecurringEvent2 @event)
			{
				throw new Exception("fail!");
			}
		}
	}
}