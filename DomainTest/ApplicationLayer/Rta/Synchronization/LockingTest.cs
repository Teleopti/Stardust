using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Synchronization
{
	[RtaTest]
	[Toggle(Toggles.RTA_NewEventHangfireRTA_34333)]
	[TestFixture]
	public class LockingTest : ISetup
	{
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Rta;
		public IDistributedLockAcquirer DistributedLock;
		public NonConcurrenctSafeHandler Handler;
		public RtaTestAttribute Context;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<NonConcurrenctSafeHandler>().For<IHandleEvent<PersonStateChangedEvent>, IInitializeble>();
		}

		[Test]
		public void ShouldNotPublishToHandlerWhileInitializing()
		{
			var personId = Guid.NewGuid();
			Database.WithUser("user", personId);
			Now.Is("2015-01-15 08:00");
			Rta.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user",
				StateCode = "state"
			});

			Context.SimulateRestartWith(Now, Database);
			var initialize = Execute.OnAnotherThread(() => Rta.SaveState(new ExternalUserStateForTest()));
			Handler.InHandler.WaitOne(TimeSpan.FromSeconds(1));
			var systemTask = Task.Factory.StartNew(() =>
			{
				using (DistributedLock.LockForTypeOf(Handler))
				{
				}
			});
			var systemTaskRanWhileInitializing = systemTask.Wait(TimeSpan.FromMilliseconds(100));
			initialize.Abort();

			systemTaskRanWhileInitializing.Should().Be.False();
		}

		public class NonConcurrenctSafeHandler :
			IHandleEvent<PersonStateChangedEvent>,
			IInitializeble
		{
			public ManualResetEvent InHandler = new ManualResetEvent(false);

			public void Handle(PersonStateChangedEvent @event)
			{
				InHandler.Set();
				new ManualResetEvent(false).WaitOne();
			}

			public bool Initialized()
			{
				return false;
			}
		}

	}
}