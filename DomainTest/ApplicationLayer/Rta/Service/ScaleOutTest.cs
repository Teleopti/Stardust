using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	[ToggleOff(Domain.FeatureFlags.Toggles.RTA_RuleMappingOptimization_39812)]
	public class ScaleOutTest
	{
		public FakeRtaDatabase Database;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;
		public MutableNow Now;
		public FakeEventPublisher Publisher;
		public IAgentStateReadModelReader Persister;

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
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});
			Target.CheckForActivityChanges(Database.TenantName());

			Database.ClearSchedule(personId);
			Database.WithSchedule(personId, phone, "phone", "2016-03-04 8:00", "2016-03-04 10:00");
			Database.WithSchedule(personId, brejk, "break", "2016-03-04 10:00", "2016-03-04 10:15");
			Target.CheckForActivityChanges(Database.TenantName());

			Database.PersistedReadModel.NextActivity.Should().Be("break");
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
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Publisher.Clear();

			Database
				.ClearRuleMap()
				.WithRule("phone", phone, 0, Adherence.In)
				.WithRule("admin", phone, 0, Adherence.Neutral);
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotCacheStateGroups()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithRule("phone", null, "Ready")
				;
			Now.Is("2016-03-04 9:00");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Publisher.Clear();

			Database
				.ClearStates()
				.ClearRuleMap()
				.WithRule("phone", null, "InCall")
				;
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Should().Not.Be.Empty();
		}
		
	}
}