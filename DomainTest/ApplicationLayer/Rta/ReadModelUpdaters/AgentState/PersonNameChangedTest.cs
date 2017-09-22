using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Helper;
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
			Persister.Has(new AgentStateReadModel {PersonId = personId, IsDeleted = false});
			
			Target.Handle(new PersonNameChangedEvent
			{
				PersonId = personId,
				FirstName = "bill",
				LastName = "gates"
			});

			var model = Persister.Models.Single();
			model.PersonId.Should().Be(personId);
			model.FirstName.Should().Be("bill");
			model.LastName.Should().Be("gates");
			model.IsDeleted.Should().Be(false);
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
		}

		[Test]
		public void ShouldInsertNameAndSetToDeleted()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonNameChangedEvent
			{
				PersonId = personId,
				FirstName = "bill",
				LastName = "gates",
				Timestamp = "2017-02-14 08:00".Utc()
			});

			var model = Persister.Models.Single();
			model.IsDeleted.Should().Be(true);
			model.FirstName.Should().Be("bill");
			model.LastName.Should().Be("gates");
		}
	}
}