using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AgentState
{
	[TestFixture]
	[ReadModelUpdaterTest]
	public class TerminatedPersonTest
	{
		public AgentStateReadModelMaintainer Target;
		public MutableNow Now;
		public FakeAgentStateReadModelPersister Persister;
		
		[Test]
		public void ShouldSetDeletedIfPersonIsTerminated()
		{
			var personId = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModel { PersonId = personId });

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = null
			});

			Persister.Models.Single().IsDeleted.Should().Be(true);
		}

		[Test]
		public void ShouldDeleteRowIfSoftDeletedForMoreThanThirtyMinutes()
		{
			var personId = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModel { PersonId = personId });
			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = null,
				Timestamp = "2016-10-04 08:00".Utc()
			});
			Now.Is("2016-10-04 08:30");
			
			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = Guid.NewGuid(),
				TeamId = Guid.NewGuid(),
				Timestamp = "2016-10-04 08:30".Utc()
			});
			
			Persister.Get(personId).Should().Be.Null();
		}

		[Test]
		public void ShouldKeepReadModelIfPersonIsInATeam()
		{
			var personId = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModel { PersonId = personId });

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = Guid.Empty
			});

			Persister.Models.Single().PersonId.Should().Be(personId);
		}
		
	}
}