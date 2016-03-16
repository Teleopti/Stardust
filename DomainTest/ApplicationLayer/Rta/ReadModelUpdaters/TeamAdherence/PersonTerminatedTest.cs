using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.TeamAdherence
{
	[ReadModelUpdaterTest]
	[TestFixture]
	[Toggle(Toggles.RTA_TerminatedPersons_36042)]
	public class PersonTerminatedTest
	{
		public FakeTeamOutOfAdherenceReadModelPersister Persister;
		public TeamOutOfAdherenceReadModelUpdater Target;

		[Test]
		public void ShouldExludeTerminatedPerson()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId, PersonId = personId });

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = null
			});

			Persister.Get(teamId).Count.Should().Be(0);
		}

		[Test]
		public void ShouldExludeFromAnySite()
		{
			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId1, PersonId = personId });
			Target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId2, PersonId = personId });

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = null
			});

			Persister.Get(teamId1).Count.Should().Be(0);
			Persister.Get(teamId2).Count.Should().Be(0);
		}

		[Test]
		public void ShouldOnlyExcludeTerminatedPerson()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId, PersonId = personId });

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = teamId
			});

			Persister.Get(teamId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldReactivatePerson()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId, PersonId = personId });
			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = null
			});

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = teamId
			});

			Persister.Get(teamId).Count.Should().Be(1);
		}
		
	}
}