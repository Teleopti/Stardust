using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Synchronization
{
	[RtaTest]
	[Toggle(Toggles.RTA_NewEventHangfireRTA_34333)]
	[TestFixture]
	public class SiteAdherenceTest
	{
		public FakeRtaDatabase Database;
		public FakeSiteOutOfAdherenceReadModelPersister Model;
		public Domain.ApplicationLayer.Rta.Service.Rta Rta;
		public MutableNow Now;
		public RtaTestAttribute Context;

		[Test]
		public void ShouldInitializeSiteAdherence()
		{
			var siteId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("user", personId, null, null, siteId)
				.WithSchedule(personId, phone, "2015-01-15 8:00", "2015-01-15 10:00")
				.WithRule("break", phone, 1);
			Now.Is("2015-01-15 08:00");
			Rta.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user", 
				StateCode = "break"
			});

			Context.SimulateRestart();
			Rta.Touch(Database.TenantName());

			Model.Get(siteId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldNotReinitializeSiteAdherenceOnInitialize()
		{
			var existingSite = Guid.NewGuid();
			var stateSite = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Model.Persist(new SiteOutOfAdherenceReadModel
			{
				Count = 3,
				SiteId = existingSite
			});
			Database
				.WithUser("user", personId, null, null, stateSite)
				.WithSchedule(personId, phone, "2015-01-15 8:00", "2015-01-15 10:00")
				.WithRule("break", phone, 1);
			Now.Is("2015-01-15 08:00");
			Rta.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user", 
				StateCode = "break"
			});

			Context.SimulateRestart();
			Rta.Touch(Database.TenantName());

			Model.Get(existingSite).Count.Should().Be(3);
			Model.Get(stateSite).Should().Be.Null();
		}
		
	}
}