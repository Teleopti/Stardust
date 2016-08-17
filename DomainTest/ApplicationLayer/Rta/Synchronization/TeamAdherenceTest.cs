using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Synchronization
{
	[RtaTest]
	[TestFixture]
	public class TeamAdherenceTest
	{
		public FakeRtaDatabase Database;
		public FakeTeamOutOfAdherenceReadModelPersister Model;
		public Domain.ApplicationLayer.Rta.Service.Rta Rta;
		public MutableNow Now;
		public RtaTestAttribute Context;

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

				.WithRule("phone", phone, 0)
				.WithRule("phone", brejk, 1)
				.WithRule("break", phone, -1)
				;
			Now.Is("2015-01-15 08:00");
			Rta.SaveState(new StateForTest {UserCode = "A1", StateCode = "phone"});
			Rta.SaveState(new StateForTest {UserCode = "A2", StateCode = "phone"});
			Rta.SaveState(new StateForTest {UserCode = "A3", StateCode = "phone"});

			Rta.SaveState(new StateForTest {UserCode = "B1", StateCode = "break"});
			Rta.SaveState(new StateForTest {UserCode = "B2", StateCode = "phone"});
			Rta.SaveState(new StateForTest {UserCode = "B3", StateCode = "phone"});

			Context.SimulateRestart();
			Rta.Touch(Database.TenantName());

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
				.WithRule("break", phone, 1);
			Now.Is("2015-01-15 08:00");
			Rta.SaveState(new StateForTest
			{
				UserCode = "user", 
				StateCode = "break"
			});

			Context.SimulateRestart();
			Rta.Touch(Database.TenantName());

			Model.Get(existingTeam).Count.Should().Be(3);
			Model.Get(stateTeam).Should().Be.Null();
		}

	}
}