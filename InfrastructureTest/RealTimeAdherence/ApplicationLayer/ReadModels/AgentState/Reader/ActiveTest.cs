using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;

namespace Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.ApplicationLayer.ReadModels.AgentState.Reader
{
	[TestFixture]
	[UnitOfWorkTest]
	public class ActiveTest
	{
		public IAgentStateReadModelReader Target;
		public IAgentStateReadModelPersister Persister;

		[Test]
		public void ShouldExludeAgentsNotActivatedByName()
		{
			var personId = Guid.NewGuid();
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = personId,
				BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value
			});

			var result = Target.Read(new AgentStateFilter());

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldIncludeAgentsActivatedByName()
		{
			var personId = Guid.NewGuid();
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = personId,
				BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value
			});
			Persister.UpsertName(personId, "roger", "kjatz");

			var result = Target.Read(new AgentStateFilter());

			result.Single().PersonId.Should().Be(personId);
		}
	}
}