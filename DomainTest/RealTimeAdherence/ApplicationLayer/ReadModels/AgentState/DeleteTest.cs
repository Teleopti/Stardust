using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using ExternalLogon = Teleopti.Ccc.Domain.ApplicationLayer.Events.ExternalLogon;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.ApplicationLayer.ReadModels.AgentState
{
	[TestFixture]
	[DomainTest]
	public class DeleteTest
	{
		public AgentStateReadModelMaintainer Target;
		public FakeAgentStateReadModelPersister Persister;

		[Test]
		public void ShouldUpsertNoAssociationWhenDeleting()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonDeletedEvent { PersonId = personId });

			Persister.Models.Single().PersonId.Should().Be(personId);
		}

	}
}