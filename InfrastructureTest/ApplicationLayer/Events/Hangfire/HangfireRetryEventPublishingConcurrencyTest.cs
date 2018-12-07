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
	public class HangfireRetryEventPublishingConcurrencyTest : IExtendSystem
	{
		public HangfireUtilities Hangfire;
		public IEventPublisher Publisher;
		public FailingHandlerImpl FailingHandler;
		public ConcurrencyRunner Run;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<FailingHandlerImpl>();
		}

		[Test]
		public void ShouldRetryDifferentAmountOfTimesConcurrently()
		{
			10.Times(() =>
			{
				Publisher.Publish(new ThreeRetryEvent());
				Publisher.Publish(new TenRetryEvent());
			});

			Hangfire.EmulateQueueProcessing();

			FailingHandler.Attempts.Should().Be(130);
		}

		public class ThreeRetryEvent : IEvent
		{
		}

		public class TenRetryEvent : IEvent
		{
		}

		public class FailingHandlerImpl :
			IHandleEvent<ThreeRetryEvent>,
			IHandleEvent<TenRetryEvent>,
			IRunOnHangfire
		{

			public int Attempts = 0;

			[Attempts(3)]
			public void Handle(ThreeRetryEvent @event)
			{
				Interlocked.Increment(ref Attempts);
				throw new Exception("fail!");
			}

			[Attempts(10)]
			public void Handle(TenRetryEvent @event)
			{
				Interlocked.Increment(ref Attempts);
				throw new Exception("fail!");
			}
		}

	}
}