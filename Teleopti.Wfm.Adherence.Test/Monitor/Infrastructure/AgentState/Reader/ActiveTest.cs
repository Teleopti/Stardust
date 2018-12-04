using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Wfm.Adherence.ApplicationLayer.ViewModels;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Infrastructure.AgentState.Reader
{
	[TestFixture]
	[UnitOfWorkTest]
	public class ActiveTest
	{
		public IAgentStateReadModelReader Target;
		public IAgentStateReadModelPersister Persister;

		[Test]
		public void ShouldExludeAgentsNotActivated()
		{
			var personId = Guid.NewGuid();
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = personId,
				BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value
			});
			Persister.UpsertNoAssociation(personId);
			
			var result = Target.Read(new AgentStateFilter());

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldIncludeAgentsActivated()
		{
			var personId = Guid.NewGuid();
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = personId,
				BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value,
				FirstName = "roger",
				LastName = "kjatz"
			});

			var result = Target.Read(new AgentStateFilter());

			result.Single().PersonId.Should().Be(personId);
		}
	}
}