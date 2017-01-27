using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AgentState
{
	[TestFixture]
	[DomainTest]
	public class PersonNameChangedTest
	{
		public AgentStateReadModelMaintainer Target;
		public FakeAgentStateReadModelPersister Persister;

		[Test]
		public void ShouldSaveFirstAndLastName()
		{
			var personId = Guid.NewGuid();
			Persister.Has(new AgentStateReadModel {PersonId = personId});
			
			Target.Handle(new PersonNameChangedEvent
			{
				PersonId = personId,
				FirstName = "bill",
				LastName = "gates"
			});

			Persister.Models.Single().PersonId.Should().Be(personId);
			Persister.Models.Single().FirstName.Should().Be("bill");
			Persister.Models.Single().LastName.Should().Be("gates");
		}

		[Test]
		public void ShouldNotSetAgentToActive()
		{
			var personId = Guid.NewGuid();
			Persister.Has(new AgentStateReadModel { PersonId = personId, IsDeleted = true});
			
			Target.Handle(new PersonNameChangedEvent
			{
				PersonId = personId,
				FirstName = "bill",
				LastName = "gates"
			});

			Persister.Models.Single().IsDeleted.Should().Be(true);
			Persister.Models.Single().FirstName.Should().Be("bill");
			Persister.Models.Single().LastName.Should().Be("gates");
		}
	}
}