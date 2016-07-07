using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.TeamAdherence
{
	[TestFixture]
	[ReadModelUpdaterTest]
	public class PersonDeletedTest
	{
		public FakeTeamOutOfAdherenceReadModelPersister Persister;
		public TeamOutOfAdherenceReadModelUpdater Target;

		[Test]
		public void ShouldExludeDeletedPerson()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId, PersonId = personId });

			Target.Handle(new PersonDeletedEvent
			{
				PersonId = personId
			});

			Persister.Get(teamId).Count.Should().Be(0);
		}

		[Test]
		public void ShouldExludeFromAnyTeam()
		{
			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId1, PersonId = personId });
			Target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId2, PersonId = personId });

			Target.Handle(new PersonDeletedEvent
			{
				PersonId = personId
			});

			Persister.Get(teamId1).Count.Should().Be(0);
			Persister.Get(teamId2).Count.Should().Be(0);
		}

		[Test]
		public void ShouldExludeFromAllTeams()
		{
			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId1, PersonId = personId });
			Target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId2, PersonId = personId });

			Target.Handle(new PersonDeletedEvent
			{
				PersonId = personId
			});

			Persister.Get(teamId1).Count.Should().Be(0);
			Persister.Get(teamId2).Count.Should().Be(0);
		}

		[Test]
		public void ShouldExludeDeletedPersonNextUpdate()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId, PersonId = personId });

			Target.Handle(new PersonDeletedEvent
			{
				PersonId = personId
			});
			Target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId, PersonId = Guid.NewGuid() });

			Persister.Get(teamId).Count.Should().Be(1);
		}
	}
}