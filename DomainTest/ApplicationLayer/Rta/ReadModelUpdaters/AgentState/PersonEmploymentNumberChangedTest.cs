using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AgentState
{
	[TestFixture]
	[DomainTest]
	[Toggle(Toggles.RTA_FasterAgentsView_42039)]
	public class PersonEmploymentNumberChangedTest
	{
		public AgentStateReadModelNamesMaintainer Target;
		public FakeAgentStateReadModelPersister Persister;

		[Test]
		public void ShouldSaveEmploymentNumber()
		{
			var personId = Guid.NewGuid();
			Persister.Has(new AgentStateReadModel {PersonId = personId});
			
			Target.Handle(new PersonEmploymentNumberChangedEvent
			{
				PersonId = personId,
				EmploymentNumber = "123"
			});

			Persister.Models.Single().PersonId.Should().Be(personId);
			Persister.Models.Single().EmploymentNumber.Should().Be("123");
		}

		[Test]
		public void ShouldNotSetAgentToActive()
		{
			var personId = Guid.NewGuid();
			Persister.Has(new AgentStateReadModel { PersonId = personId, IsDeleted = true});
			
			Target.Handle(new PersonEmploymentNumberChangedEvent
			{
				PersonId = personId,
				EmploymentNumber = "123"
			});

			Persister.Models.Single().IsDeleted.Should().Be(true);
			Persister.Models.Single().EmploymentNumber.Should().Be("123");
		}
	}
}