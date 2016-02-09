using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.TeamAdherence
{
	[ReadModelUpdaterTest]
	[TestFixture]
	[Toggle(Toggles.RTA_TeamChanges_36043)]
	public class TeamChanges
	{
		public FakeTeamOutOfAdherenceReadModelPersister Persister;
		public TeamOutOfAdherenceReadModelUpdater Target;

		[Test]
		public void ShouldExcludePersonChangingTeam()
		{
			var originTeam = Guid.NewGuid();
			var destinationTeam = Guid.NewGuid();
			var movingPerson = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { TeamId = originTeam, PersonId = movingPerson });
			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = movingPerson,
				TeamId = destinationTeam
			});

			Persister.Get(originTeam).Count.Should().Be(0);
		}

		[Test]
		public void ShouldIncludeInDestinationTeam()
		{
			var destinationTeam = Guid.NewGuid();
			var movingPerson = Guid.NewGuid();
			Target.Handle(new PersonInAdherenceEvent { TeamId = destinationTeam, PersonId = Guid.NewGuid() });

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = movingPerson,
				TeamId = destinationTeam
			});
			Target.Handle(new PersonOutOfAdherenceEvent { TeamId = destinationTeam, PersonId = movingPerson });

			Persister.Get(destinationTeam).Count.Should().Be(1);
		}
		
	}
}