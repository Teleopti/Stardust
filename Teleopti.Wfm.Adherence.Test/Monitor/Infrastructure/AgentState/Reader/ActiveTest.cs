using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.Monitor;
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
		public ICurrentBusinessUnit CurrentBusinessUnit;

		[Test]
		public void ShouldExludeAgentsNotActivated()
		{
			var personId = Guid.NewGuid();
			Persister.UpsertAssociation(new AssociationInfo
			{
				BusinessUnitId = CurrentBusinessUnit.CurrentId(),
				PersonId = personId,
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
				BusinessUnitId = CurrentBusinessUnit.CurrentId(),
				PersonId = personId,
				FirstName = "roger",
				LastName = "kjatz"
			});

			var result = Target.Read(new AgentStateFilter());

			result.Single().PersonId.Should().Be(personId);
		}
	}
}