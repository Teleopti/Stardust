using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AgentState
{
	[TestFixture]
	[DomainTest]
	public class PersonEmploymentNumberChangedTest
	{
		public AgentStateReadModelMaintainer Target;
		public FakeAgentStateReadModelPersister Persister;

		[Test]
		public void ShouldUpdateEmploymentNumber()
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
		public void ShouldInsertEmploymentNumberAndSetToDeleted()
		{
			var personId = Guid.NewGuid();
			
			Target.Handle(new PersonEmploymentNumberChangedEvent
			{
				PersonId = personId,
				EmploymentNumber = "123",
				Timestamp = "2017-02-14 08:00".Utc()
			});

			Persister.Models.Single().EmploymentNumber.Should().Be("123");
			Persister.Models.Single().IsDeleted.Should().Be(true);
			Persister.Models.Single().ExpiresAt.Should().Be("2017-02-21 08:00".Utc());
		}
	}
}