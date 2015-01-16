using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Synchronization
{
	[RtaTest]
	[Toggle(Toggles.RTA_SeeAdherenceDetailsForOneAgent_31285)]
	[Toggle(Toggles.RTA_SeePercentageAdherenceForOneAgent_30783)]
	[TestFixture]
	public class InitializeProcessTest
	{
		public FakeRtaDatabase Database;
		public IStateStreamSynchronizer Target;
		public FakeAdherencePercentageReadModelPersister Persister;
		public IRta Rta;
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
			Database.WithUser("user", personId);
			Now.Is("2015-01-15 08:00");
			Rta.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user",
				StateCode = "state"
			});
			EventPublisher.PublishedEvents.Clear();

			Target.Initialize();
			Rta.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user",
				StateCode = "anotherstate"
			});

			EventPublisher.PublishedEvents.Should().Have.Count.GreaterThan(0);
		}

		[Test]
		public void ShouldPublishEventsWhileInitializing()
		{
			var personId = Guid.NewGuid();
			Database.WithUser("user", personId);
			Now.Is("2015-01-15 08:00");
			Rta.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user",
				StateCode = "state"
			});
			EventPublisher.PublishedEvents.Clear();

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
			Task.WaitAll(initialize, systemTask);

			EventPublisher.PublishedEvents.OfType<TestEvent>().Should().Have.Count.EqualTo(100000);
		}

		public class TestEvent : IEvent
		{
		}
	}

	public static class Extensions
	{
		public static void Times(this int times, Action<int> action)
		{
			Enumerable.Range(0, times).ForEach(action);
		}
	}
}