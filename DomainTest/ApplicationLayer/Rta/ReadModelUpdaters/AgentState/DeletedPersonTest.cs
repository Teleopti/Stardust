using System;
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
	[Toggle(Toggles.RTA_DeletedPersons_36041)]
	public class DeletedPersonTest
	{
		public AgentStateReadModelUpdater Target;
		public FakeAgentStateReadModelStorage Persister;

		[Test]
		public void ShouldRemoveReadModelWhenPersonIsDeleted()
		{
			var personId = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModel {PersonId = personId});

			Target.Handle(new PersonDeletedEvent {PersonId = personId});

			Persister.Models.Should().Be.Empty();
		}
	}
}