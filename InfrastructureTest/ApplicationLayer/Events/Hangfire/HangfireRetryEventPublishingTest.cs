using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop.Core;
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
	[RealHangfireTest]
	public class HangfireRetryEventPublishingTest : IExtendSystem
	{
		public HangfireUtilities Hangfire;
		public IEventPublisher Publisher;
		public IRecurringEventPublisher Recurring;
		public FailingHandlerImpl FailingHandler;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<FailingAfterAspect>();
			extend.AddService<FailingHandlerImpl>();
		}
		
		[Test]
		public void ShouldScheduleRetry()
		{
			Publisher.Publish(new ThreeRetryEvent());

			Hangfire.EmulateWorkerIteration();

			Hangfire.NumberOfScheduledJobs().Should().Be(1);
			Hangfire.NumberOfJobsInQueue(Queues.Default).Should().Be(0);
		}

		[Test]
		public void ShouldScheduleSecondAttempt()
		{
			Publisher.Publish(new ThreeRetryEvent());

			Hangfire.EmulateWorkerIteration();
			Hangfire.RequeueScheduledJobs();
			Hangfire.EmulateWorkerIteration();

			Hangfire.NumberOfScheduledJobs().Should().Be(1);
		}

		[Test]
		public void ShouldFailOnLastAttempt()
		{
			Publisher.Publish(new ThreeRetryEvent());

			Hangfire.EmulateWorkerIteration();
			Hangfire.RequeueScheduledJobs();
			Hangfire.EmulateWorkerIteration();
			Hangfire.RequeueScheduledJobs();
			Hangfire.EmulateWorkerIteration();

			Hangfire.NumberOfFailedJobs().Should().Be(1);
			Hangfire.NumberOfScheduledJobs().Should().Be(0);
		}

		[Test]
		public void ShouldSucceedOnSecondAttempt()
		{
			Publisher.Publish(new ThreeRetryEvent());

			Hangfire.EmulateWorkerIteration();
			Hangfire.RequeueScheduledJobs();
			FailingHandler.Succeeds = true;
			Hangfire.EmulateWorkerIteration();

			Hangfire.NumberOfFailedJobs().Should().Be(0);
			Hangfire.NumberOfScheduledJobs().Should().Be(0);
		}

		[Test]
		public void ShouldRetry3TimesByDefault()
		{
			Publisher.Publish(new DefaultRetryEvent());

			10.Times(() =>
			{
				Hangfire.EmulateWorkerIteration();
				Hangfire.RequeueScheduledJobs();
			});

			FailingHandler.Attempts.Should().Be(3);
		}

		[Test]
		public void ShouldNotRetryMinutelyRecurringJobs()
		{
			Recurring.PublishMinutely(new MinutelyEvent());

			Hangfire.TriggerRecurringJobs();
			20.Times(() =>
			{
				Hangfire.EmulateWorkerIteration();
				Hangfire.RequeueScheduledJobs();
			});

			FailingHandler.Attempts.Should().Be(1);
		}

		[Test]
		public void ShouldRetry10TimesWhenAspectFails()
		{
			Publisher.Publish(new TenRetryAspectEvent());

			11.Times(() =>
			{
				Hangfire.EmulateWorkerIteration();
				Hangfire.RequeueScheduledJobs();
			});

			FailingHandler.Attempts.Should().Be(10);
		}
		
		public class ThreeRetryEvent : IEvent
		{
		}

		public class DefaultRetryEvent : IEvent
		{
		}

		public class MinutelyEvent : IEvent
		{
		}

		public class TenRetryAspectEvent : IEvent
		{
		}

		public class FailingHandlerImpl :
			IHandleEvent<ThreeRetryEvent>,
			IHandleEvent<DefaultRetryEvent>,
			IHandleEvent<MinutelyEvent>,
			IHandleEvent<TenRetryAspectEvent>,
			IRunOnHangfire
		{

			public int Attempts = 0;
			public bool Succeeds = false;

			[Attempts(3)]
			public void Handle(ThreeRetryEvent @event)
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

			public void Handle(MinutelyEvent @event)
			{
				Attempts++;
				if (!Succeeds)
					throw new Exception("fail!");
			}

			[FailingAfterAspect]
			[Attempts(10)]
			public virtual void Handle(TenRetryAspectEvent @event)
			{
				Attempts++;
			}
		}

		public class FailingAfterAspectAttribute : AspectAttribute
		{
			public FailingAfterAspectAttribute() : base(typeof(FailingAfterAspect))
			{
			}
		}

		public class FailingAfterAspect : IAspect
		{
			public void OnBeforeInvocation(IInvocationInfo invocation)
			{
			}

			public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
			{
				throw new NotImplementedException();
			}
		}
	}
}