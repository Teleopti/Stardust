using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AgentState
{
	[TestFixture]
	[ReadModelUpdaterTest]
	[Toggle(Toggles.RTA_TerminatedPersons_36042)]
	public class TerminatedPersonTest
	{
		public AgentStateReadModelUpdater Target;
		public FakeAgentStateReadModelStorage Persister;

		[Test]
		public void ShouldRemoveReadModelIfPersonIsTerminated()
		{
			var personId = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModel { PersonId = personId });

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = null
			});

			Persister.Models.Should().Be.Empty();
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