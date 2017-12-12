using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AdherencePercentage
{
	[DomainTest]
	[TestFixture]
	[ToggleOff(Toggles.RTA_ViewHistoricalAhderenceForRecentShifts_46786)]
	public class DeletedTest
	{
		public FakeAdherencePercentageReadModelPersister Persister;
		public AdherencePercentageReadModelUpdater Target;

		[Test]
		public void ShouldDeletePerson()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent {PersonId = personId});

			Target.Handle(new PersonDeletedEvent {PersonId = personId});

			Persister.PersistedModel.Should().Be.Null();
		}
	}
}