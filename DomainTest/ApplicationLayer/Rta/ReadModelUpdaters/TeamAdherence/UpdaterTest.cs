using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.TeamAdherence
{
	[TestFixture]
	[ReadModelUpdaterTest]
	public class UpdaterTest
	{
		public FakeTeamOutOfAdherenceReadModelPersister persister;
		public TeamOutOfAdherenceReadModelUpdater target;

		[Test]
		public void ShouldPersistTeamAdherenceOnInAdherence()
		{
			var teamId = Guid.NewGuid();

			target.Handle(new PersonInAdherenceEvent() {TeamId = teamId});

			persister.Get(teamId).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistTeamAdherenceInOutOfAdherence()
		{
			var teamId = Guid.NewGuid();

			target.Handle(new PersonOutOfAdherenceEvent() { TeamId = teamId });

			persister.Get(teamId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldExcludePersonGoingInAdherence()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();

			target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId, PersonId = personId });
			target.Handle(new PersonInAdherenceEvent { TeamId = teamId, PersonId = personId });

			persister.Get(teamId).Count.Should().Be(0);
		}

		[Test]
		public void ShouldIncludePersonGoingOutOfAdherence()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();

			target.Handle(new PersonInAdherenceEvent { TeamId = teamId, PersonId = personId });
			target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId, PersonId = personId });

			persister.Get(teamId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldCountPersonAsOutOfAdherenceWhenGoingOutInOut()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();

			target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId, PersonId = personId });
			target.Handle(new PersonInAdherenceEvent { TeamId = teamId, PersonId = personId });
			target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId, PersonId = personId });

			persister.Get(teamId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldCountPersonAsInAdherenceWhenGoingInOutIn()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();

			target.Handle(new PersonInAdherenceEvent { TeamId = teamId, PersonId = personId });
			target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId, PersonId = personId });
			target.Handle(new PersonInAdherenceEvent { TeamId = teamId, PersonId = personId });

			persister.Get(teamId).Count.Should().Be(0);
		}

		[Test]
		public void ShouldCountPersonAsNeutralAdherenceWhenGoingOutNeutral()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();

			target.Handle(new PersonOutOfAdherenceEvent {TeamId = teamId, PersonId = personId});
			target.Handle(new PersonNeutralAdherenceEvent {TeamId = teamId, PersonId = personId});

			persister.Get(teamId).Count.Should().Be(0);
		}

		[Test]
		public void ShouldCountPersonAsOutOfAdherenceWhenGoingOutNeutralOut()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();

			target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId, PersonId = personId });
			target.Handle(new PersonNeutralAdherenceEvent { TeamId = teamId, PersonId = personId });
			target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId, PersonId = personId });

			persister.Get(teamId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldSummarizePersonsOutOfAdherence()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();

			target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId, PersonId = Guid.NewGuid() });
			target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId, PersonId = personId });
			target.Handle(new PersonInAdherenceEvent { TeamId = teamId, PersonId = personId });

			persister.Get(teamId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldSummarizePersonsOutOfAdherenceForEachTeam()
		{
			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();
			
			target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId1, PersonId = Guid.NewGuid() });
			target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId2, PersonId = Guid.NewGuid() });
			target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId1, PersonId = Guid.NewGuid() });

			persister.Get(teamId1).Count.Should().Be(2);
			persister.Get(teamId2).Count.Should().Be(1);
		}

		[Test]
		public void ShouldExcludePersonGoingInAdherenceForTheFirstTime()
		{
			var teamId = Guid.NewGuid();

			target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId, PersonId = Guid.NewGuid()});
			target.Handle(new PersonInAdherenceEvent { TeamId = teamId, PersonId = Guid.NewGuid()});

			persister.Get(teamId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldNeverCountNegativePersonsBeingOutOfAdherence()
		{
			var teamId = Guid.NewGuid();

			target.Handle(new PersonInAdherenceEvent { TeamId = teamId, PersonId = Guid.NewGuid() });
			target.Handle(new PersonInAdherenceEvent { TeamId = teamId, PersonId = Guid.NewGuid() });

			persister.Get(teamId).Count.Should().Be(0);
		}

		[Test]
		public void ShouldPersistWithSiteIdOnInAdherence()
		{
			var teamId = Guid.NewGuid();
			var siteId = Guid.NewGuid();

			target.Handle(new PersonInAdherenceEvent() { TeamId = teamId, SiteId = siteId});

			persister.Read(siteId).Single().TeamId.Should().Be(teamId);
		}

		[Test]
		public void ShouldPersistWithSiteIdOnOutOfAdherence()
		{
			var teamId = Guid.NewGuid();
			var siteId = Guid.NewGuid();

			target.Handle(new PersonOutOfAdherenceEvent{ TeamId = teamId, SiteId = siteId});

			persister.Read(siteId).Single().TeamId.Should().Be(teamId);
		}

	}
}