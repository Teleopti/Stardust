using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	[Toggle(Toggles.RTA_ScaleOut_36979)]
	[Toggle(Toggles.RTA_NeutralAdherence_30930)]
	public class ScaleOutTest
	{
		public FakeRtaDatabase Database;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;
		public MutableNow Now;
		public FakeEventPublisher EventPublisher;
		
		[Test]
		public void ShouldNotCacheSchedules()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var brejk = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2016-03-04 8:00", "2016-03-04 10:15")
				;
			Now.Is("2016-03-04 9:00");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});
			Target.CheckForActivityChanges(Database.TenantName());

			Database.ClearSchedule(personId);
			Database.WithSchedule(personId, phone, "phone", "2016-03-04 8:00", "2016-03-04 10:00");
			Database.WithSchedule(personId, brejk, "break", "2016-03-04 10:00", "2016-03-04 10:15");
			Target.CheckForActivityChanges(Database.TenantName());

			Database.PersistedReadModel.ScheduledNext.Should().Be("break");
		}

		[Test]
		public void ShouldNotCacheRules()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2016-03-04 8:00", "2016-03-04 10:15")
				.WithRule("phone", phone, 0, Adherence.In)
				.WithRule("admin", phone, 0, Adherence.Out)
				;
			Now.Is("2016-03-04 9:00");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			EventPublisher.Clear();

			Database
				.ClearRuleMap()
				.WithRule("phone", phone, 0, Adherence.In)
				.WithRule("admin", phone, 0, Adherence.Neutral);
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			EventPublisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>().Should().Not.Be.Empty();
		}

		[Test]
		[Toggle(Toggles.RTA_AdherenceDetails_34267)]
		public void ShouldNotCacheStateGroups()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithStateGroup("phone", "Ready")
				;
			Now.Is("2016-03-04 9:00");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			EventPublisher.Clear();

			Database
				.ClearStateGroups()
				.WithStateGroup("phone", "InCall");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			EventPublisher.PublishedEvents.OfType<PersonStateChangedEvent>().Should().Not.Be.Empty();
		}

	}
}