using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.SiteAdherence
{
	[ReadModelUpdaterTest]
	[TestFixture]
	public class OptimizeReadingDataFromDatabase
	{
		public FakeSiteOutOfAdherenceReadModelPersister Persister;
		public SiteOutOfAdherenceReadModelUpdater Target;
		
		[Test]
		public void ShouldExcludePersonFromPreviousSites()
		{
			var businessUnitId = Guid.NewGuid();
			var previousSite1 = Guid.NewGuid();
			var previousSite2 = Guid.NewGuid();
			var destinationSite = Guid.NewGuid();
			var movingPerson = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = movingPerson,
				BusinessUnitId = businessUnitId,
				SiteId = previousSite1,
			});
			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = movingPerson,
				BusinessUnitId = businessUnitId,
				SiteId = previousSite2,
			});

			Target.Handle(new PersonAssociationChangedEvent
			{
				Version = 2,
				PersonId = movingPerson,
				BusinessUnitId = businessUnitId,
				SiteId = destinationSite,
				PreviousAssociation = new[]
				{
					new Association
					{
						BusinessUnitId = businessUnitId,
						SiteId = previousSite1
					},
					new Association
					{
						BusinessUnitId = businessUnitId,
						SiteId = previousSite2
					}
				}
			});

			Persister.Get(previousSite1).Count.Should().Be(0);
			Persister.Get(previousSite2).Count.Should().Be(0);
		}
		
		[Test]
		public void ShouldIncludeInNewSite()
		{
			var businessUnitId = Guid.NewGuid();
			var originSite = Guid.NewGuid();
			var destinationSite = Guid.NewGuid();
			var movingPerson = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent { PersonId = movingPerson, BusinessUnitId = businessUnitId, SiteId = originSite,  });

			Target.Handle(new PersonAssociationChangedEvent
			{
				Version = 2,
				PersonId = movingPerson,
				BusinessUnitId = businessUnitId,
				SiteId = destinationSite
			});
			Target.Handle(new PersonOutOfAdherenceEvent { PersonId = movingPerson, BusinessUnitId = businessUnitId, SiteId = destinationSite,  });

			Persister.Get(destinationSite).Count.Should().Be(1);
		}


		[Test]
		public void ShouldExcludeWhenTerminated()
		{
			var businessUnitId = Guid.NewGuid();
			var previousSite = Guid.NewGuid();
			var movingPerson = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = movingPerson,
				BusinessUnitId = businessUnitId,
				SiteId = previousSite,
			});

			Target.Handle(new PersonAssociationChangedEvent
			{
				Version = 2,
				PersonId = movingPerson,
				PreviousAssociation = new[]
				{
					new Association
					{
						BusinessUnitId = businessUnitId,
						SiteId = previousSite
					}
				}
			});

			Persister.Get(previousSite).Count.Should().Be(0);
		}

	}
}