using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.ApplicationLayer.ReadModels.AgentState
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

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = Guid.NewGuid(),
				ExternalLogons = new[] {new ExternalLogon()},
				EmploymentNumber = "123"
			});

			Persister.Models.Single().PersonId.Should().Be(personId);
			Persister.Models.Single().EmploymentNumber.Should().Be("123");
		}
	}
}