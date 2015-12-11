using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
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
		public FakeAdherencePercentageReadModelPersister Persister;
		public Domain.ApplicationLayer.Rta.Service.Rta Rta;
		public MutableNow Now;
		public FakeMessageSender MessageSender;
		public FakeEventPublisher EventPublisher;
		public ICurrentEventPublisher CurrentEventPublisher;
		public RtaTestAttribute Context;

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

			Context.SimulateRestart();
			Rta.SaveState(new ExternalUserStateForTest());

			MessageSender.AllNotifications.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldPublishEventsAfterInitialize()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("user", personId)
				.WithRule("state", Guid.NewGuid())
				.WithRule("anotherstate", Guid.NewGuid());
			Now.Is("2015-01-15 08:00");
			Rta.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user",
				StateCode = "state"
			});
			EventPublisher.Clear();

			Context.SimulateRestart();
			Rta.SaveState(new ExternalUserStateForTest());
			Rta.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user",
				StateCode = "anotherstate"
			});

			EventPublisher.PublishedEvents.Should().Have.Count.GreaterThan(0);
		}

		[Test]
		public void ShouldNotAddExistingStateCode()
		{
			var personId = Guid.NewGuid();
			var platformId = Guid.NewGuid();
			var buId = Guid.NewGuid();
			Now.Is("2015-10-01 08:00");
			Database
				.WithBusinessUnit(buId)
				.WithUser("user", personId)
				.WithDefaultStateGroup()
				.WithStateCode("statecode", platformId.ToString())
				;
			Database.PersistActualAgentReadModel(new AgentStateReadModel
			{
				PersonId = personId,
				StateCode = "statecode",
				PlatformTypeId = platformId,
				BusinessUnitId = buId
			});

			Context.SimulateRestart();
			Rta.SaveState(new ExternalUserStateForTest());

			Database.AddedStateCodes.Should().Have.Count.EqualTo(1);
		}
	}
}