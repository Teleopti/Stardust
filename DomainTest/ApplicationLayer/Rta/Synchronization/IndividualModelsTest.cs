using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Synchronization
{
	[RtaTest]
	[Toggle(Toggles.RTA_NewEventHangfireRTA_34333)]
	[TestFixture]
	public class IndividualModelsTest
	{
		public FakeRtaDatabase Database;
		public IStateStreamSynchronizer Target;
		public FakeSiteOutOfAdherenceReadModelPersister SiteOutOfAdherenceReadModel;
		public FakeTeamOutOfAdherenceReadModelPersister TeamOutOfAdherenceReadModel;
		public IRta Rta;
		public MutableNow Now;

		[Test]
		public void ShouldInitializeModelsWithoutData()
		{
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			TeamOutOfAdherenceReadModel.Persist(new TeamOutOfAdherenceReadModel
			{
				TeamId = teamId,
				Count = 3
			});
			Database
				.WithUser("user", personId, null, null, siteId)
				.WithSchedule(personId, phone, "2015-01-15 8:00", "2015-01-15 10:00")
				.WithAlarm("break", phone, 1);
			Now.Is("2015-01-15 08:00");
			Rta.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user", 
				StateCode = "break"
			});

			Target.Initialize();

			TeamOutOfAdherenceReadModel.Get(teamId).Count.Should().Be(3);
			SiteOutOfAdherenceReadModel.Get(siteId).Count.Should().Be(1);
		}

	}
}