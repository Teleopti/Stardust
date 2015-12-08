using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.TeamAdherence
{
	[TestFixture]
	[AdherenceTest]
	[Toggle(Toggles.RTA_DeletedPersons_36041)]
	public class PersonDeletedTest
	{
		public FakeTeamOutOfAdherenceReadModelPersister Persister;
		public TeamOutOfAdherenceReadModelUpdater Target;

		[Test]
		public void ShouldRecalculateForPersonsTeam()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId, PersonId = personId });

			Target.Handle(new PersonDeletedEvent { PersonId = personId, PersonPeriodsBefore = new [] {new PersonPeriodDetail {TeamId = teamId} }});

			Persister.Get(teamId).Count.Should().Be(0);
		}

		[Test]
		public void ShouldNotRecalculateOtherTeam()
		{
			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId1, PersonId = Guid.NewGuid() });
			Target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId2, PersonId = personId });

			Target.Handle(new PersonDeletedEvent { PersonId = personId, PersonPeriodsBefore = new[] { new PersonPeriodDetail { TeamId = teamId1 } } });

			Persister.Get(teamId1).Count.Should().Be(1);
		}

		[Test]
		public void ShouldRecalculateForAllPersonsTeams()
		{
			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId1, PersonId = personId });
			Target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId2, PersonId = personId });

			Target.Handle(new PersonDeletedEvent
			{
				PersonId = personId,
				PersonPeriodsBefore = new[] {new PersonPeriodDetail {TeamId = teamId1}, new PersonPeriodDetail {TeamId = teamId2}}
			});

			Persister.Get(teamId1).Count.Should().Be(0);
			Persister.Get(teamId2).Count.Should().Be(0);
		}

		[Test]
		public void ShouldNotIncludeDeletedPersonInNextRecalculation()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId, PersonId = personId });

			Target.Handle(new PersonDeletedEvent { PersonId = personId, PersonPeriodsBefore = new[] { new PersonPeriodDetail { TeamId = teamId } } });
			Target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId, PersonId = new Guid() });

			Persister.Get(teamId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldRememberPersonWasDeletedForACoupleOfMinutes()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId, PersonId = personId, Timestamp = "2015-12-04 08:00".Utc() });

			Target.Handle(new PersonDeletedEvent
			{
				PersonId = personId,
				PersonPeriodsBefore = new[] {new PersonPeriodDetail {TeamId = teamId}},
				Timestamp = "2015-12-04 10:00".Utc()
			});
			Target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId, PersonId = personId, Timestamp = "2015-12-04 10:05".Utc() });

			Persister.Get(teamId).Count.Should().Be(0);
		}
	}
}