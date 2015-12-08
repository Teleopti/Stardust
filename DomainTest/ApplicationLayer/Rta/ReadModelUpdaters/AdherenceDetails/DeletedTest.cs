using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AdherenceDetails
{
	[AdherenceTest]
	[TestFixture]
	public class DeletedTest
	{
		public FakeAdherenceDetailsReadModelPersister Persister;
		public AdherenceDetailsReadModelUpdater Target;

		[Ignore]
		[Test]
		public void ShouldDeletePerson()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-12-08 08:00".Utc()
			});

			Target.Handle(new PersonDeletedEvent {PersonId = personId});

			Persister.Model.Should().Be.Null();
		}
	}
}