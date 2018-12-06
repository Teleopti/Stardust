using System;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire
{
	[TestFixture]
	[RealHangfireTest]
	public class HangfireAllowRecurringFailuresEventPublishingConcurrencyTest : IExtendSystem
	{
		public HangfireUtilities Hangfire;
		public IEventPublisher Publisher;
		public IRecurringEventPublisher Recurring;
		public FailingHandlerImpl FailingHandler;
		public ConcurrencyRunner Run;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<FailingHandlerImpl>();
		}

		[Test]
		public void ShouldOnlyAllowFailuresOnRecurringConcurrently()
		{
			Recurring.PublishMinutely(new RecurringEvent());
			200.Times(() =>
			{
				Hangfire.TriggerRecurringJobs();
				Publisher.Publish(new RegularEvent());
			});

			Hangfire.EmulateQueueProcessing();

			Hangfire.NumberOfFailedJobs().Should().Be(200);
		}

		public class RegularEvent : IEvent
		{
		}

		public class RecurringEvent : IEvent
		{
		}

		public class FailingHandlerImpl :
			IHandleEvent<RecurringEvent>,
			IHandleEvent<RegularEvent>,
			IRunOnHangfire
		{
			public int Attempts = 0;

			[AllowFailures(10000)]
			public void Handle(RecurringEvent @event)
			{
				Interlocked.Increment(ref Attempts);
				throw new Exception("fail!");
			}

			[Attempts(1)]
			public void Handle(RegularEvent @event)
			{
				Interlocked.Increment(ref Attempts);
				throw new Exception("fail!");
			}
		}
		
	}
}