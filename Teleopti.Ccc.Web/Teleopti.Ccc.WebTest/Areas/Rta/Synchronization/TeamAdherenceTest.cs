using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Synchronization
{
	[RtaTest]
	[Toggle(Toggles.RTA_SeeAdherenceDetailsForOneAgent_31285)]
	[Toggle(Toggles.RTA_SeePercentageAdherenceForOneAgent_30783)]
	[Toggle(Toggles.RTA_NoBroker_31237)]
	[Toggle(Toggles.RTA_EventStreamInitialization_31237)]
	[TestFixture]
	public class TeamAdherenceTest
	{
		public FakeRtaDatabase Database;
		public IStateStreamSynchronizer Target;
		public FakeTeamOutOfAdherenceReadModelPersister Model;
		public IRta Rta;
		public MutableNow Now;

		[Test]
		public void ShouldInitializeTeamAdherence()
		{
			var teamIdA = Guid.NewGuid();
			var teamIdB = Guid.NewGuid();
			var personIdA1 = Guid.NewGuid();
			var personIdA2 = Guid.NewGuid();
			var personIdA3 = Guid.NewGuid();
			var personIdB1 = Guid.NewGuid();
			var personIdB2 = Guid.NewGuid();
			var personIdB3 = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var brejk = Guid.NewGuid();
			
			Database
				.WithUser("A1", personIdA1, null, teamIdA, null)
				.WithUser("A2", personIdA2, null, teamIdA, null)
				.WithUser("A3", personIdA3, null, teamIdA, null)
				.WithUser("B1", personIdB1, null, teamIdB, null)
				.WithUser("B2", personIdB2, null, teamIdB, null)
				.WithUser("B3", personIdB3, null, teamIdB, null)
				
				.WithSchedule(personIdA1, brejk, "2015-01-15 08:00", "2015-01-15 10:00")
				.WithSchedule(personIdA2, brejk, "2015-01-15 08:00", "2015-01-15 10:00")
				.WithSchedule(personIdA3, phone, "2015-01-15 08:00", "2015-01-15 10:00")

				.WithSchedule(personIdB1, phone, "2015-01-15 08:00", "2015-01-15 10:00")
				.WithSchedule(personIdB2, phone, "2015-01-15 08:00", "2015-01-15 10:00")
				.WithSchedule(personIdB3, phone, "2015-01-15 08:00", "2015-01-15 10:00")
				
				.WithAlarm("phone", phone, 0)
				.WithAlarm("phone", brejk, 1)
				.WithAlarm("break", phone, -1)
				;
			Now.Is("2015-01-15 08:00");
			Rta.SaveState(new ExternalUserStateForTest {UserCode = "A1", StateCode = "phone"});
			Rta.SaveState(new ExternalUserStateForTest {UserCode = "A2", StateCode = "phone"});
			Rta.SaveState(new ExternalUserStateForTest {UserCode = "A3", StateCode = "phone"});

			Rta.SaveState(new ExternalUserStateForTest {UserCode = "B1", StateCode = "break"});
			Rta.SaveState(new ExternalUserStateForTest {UserCode = "B2", StateCode = "phone"});
			Rta.SaveState(new ExternalUserStateForTest {UserCode = "B3", StateCode = "phone"});

			Target.Initialize();

			Model.Get(teamIdA).Count.Should().Be(2);
			Model.Get(teamIdB).Count.Should().Be(1);
		}

		[Test]
		public void ShouldNotReinitializeTeamAdherenceOnInitialize()
		{
			var existingTeam = Guid.NewGuid();
			var stateTeam = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Model.Persist(new TeamOutOfAdherenceReadModel
			{
				Count = 3,
				TeamId = existingTeam
			});
			Database
				.WithUser("user", personId, null, stateTeam, null)
				.WithSchedule(personId, phone, "2015-01-15 08:00", "2015-01-15 10:00")
				.WithAlarm("break", phone, 1);
			Now.Is("2015-01-15 08:00");
			Rta.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user", 
				StateCode = "break"
			});

			Target.Initialize();

			Model.Get(existingTeam).Count.Should().Be(3);
			Model.Get(stateTeam).Should().Be.Null();
		}

		[Test]
		public void ShouldReinitializeTeamAdherenceOnSync()
		{
			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Model.Persist(new TeamOutOfAdherenceReadModel
			{
				Count = 3,
				TeamId = teamId1
			});
			Model.Persist(new TeamOutOfAdherenceReadModel
			{
				Count = 3,
				TeamId = teamId2
			});
			Database
				.WithUser("user", personId, null, teamId1, null)
				.WithSchedule(personId, phone, "2015-01-15 08:00", "2015-01-15 10:00")
				.WithAlarm("break", phone, 1);
			Now.Is("2015-01-15 08:00");
			Rta.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user", 
				StateCode = "break"
			});
			Target.Sync();

			Model.Get(teamId1).Count.Should().Be(1);
			Model.Get(teamId2).Should().Be.Null();
		}

	}
}