using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Synchronization
{
	[RtaTest]
	[Toggle(Toggles.RTA_NewEventHangfireRTA_34333)]
	[TestFixture]
	public class InitializeProcessTest
	{
		public FakeRtaDatabase Database;
		public IStateStreamSynchronizer Target;
		public FakeAdherencePercentageReadModelPersister Persister;
		public Domain.ApplicationLayer.Rta.Service.Rta Rta;
		public MutableNow Now;
		public FakeMessageSender MessageSender;
		public FakeEventPublisher EventPublisher;
		public ICurrentEventPublisher CurrentEventPublisher;


		[Test]
		public void ShouldNotSendAnyMessages()
		{
			var personId = Guid.NewGuid();
			Database.WithUser("user", personId);
			Now.Is("2015-01-15 08:00");
			Rta.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user",
				StateCode = "state"
			});
			MessageSender.AllNotifications.Clear();

			Target.Initialize();

			MessageSender.AllNotifications.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldPublishEventsAfterInitialize()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("user", personId)
				.WithAlarm("state", Guid.NewGuid())
				.WithAlarm("anotherstate", Guid.NewGuid());
			Now.Is("2015-01-15 08:00");
			Rta.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user",
				StateCode = "state"
			});
			EventPublisher.Clear();

			Target.Initialize();
			Rta.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user",
				StateCode = "anotherstate"
			});

			EventPublisher.PublishedEvents.Should().Have.Count.GreaterThan(0);
		}

		[Test]
		public async void ShouldPublishEventsWhileInitializing()
		{
			var personId = Guid.NewGuid();
			Database.WithUser("user", personId);
			Now.Is("2015-01-15 08:00");
			Rta.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user",
				StateCode = "state"
			});
			EventPublisher.Clear();

			var initialize = Task.Factory.StartNew(() =>
			{
				200.Times(i =>
				{
					Persister.Clear();
					Target.Initialize();
				});
			});
			var systemTask = Task.Factory.StartNew(() =>
			{
				100000.Times(i =>
				{
					CurrentEventPublisher.Current().Publish(new TestEvent());
				});
			});
			await Task.WhenAll(initialize, systemTask);

			EventPublisher.PublishedEvents.OfType<TestEvent>().Should().Have.Count.EqualTo(100000);
		}

		

		public class TestEvent : IEvent
		{
		}


	}
}