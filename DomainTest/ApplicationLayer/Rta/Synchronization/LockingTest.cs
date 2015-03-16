using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
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
	[Toggle(Toggles.RTA_SeeAdherenceDetailsForOneAgent_31285)]
	[Toggle(Toggles.RTA_SeePercentageAdherenceForOneAgent_30783)]
	[Toggle(Toggles.RTA_EventStreamInitialization_31237)]
	[TestFixture]
	public class LockingTest : IRegisterInContainer
	{
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public IRta Rta;
		public IStateStreamSynchronizer Target;
		public IDistributedLockAcquirer DistributedLock;
		public NonConcurrenctSafeHandler Handler;

		public void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterType<NonConcurrenctSafeHandler>()
				.As<IHandleEvent<PersonStateChangedEvent>>()
				.As<IInitializeble>()
				.AsSelf()
				.SingleInstance()
				;
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

			var initialize = Execute.OnAnotherThread(() => Target.Initialize());
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