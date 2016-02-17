using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[MultiDatabaseTest]
	public class AgentStateReadModelPersisterTest
	{
		public IAgentStateReadModelPersister Target;
		public IAgentStateReadModelReader Reader;

		[Test]
		public void ShouldDelete()
		{
			var personId = Guid.NewGuid();
			var model = new AgentStateReadModelForTest {PersonId = personId};
			Target.PersistActualAgentReadModel(model);

			Target.Delete(personId);

			Reader.GetCurrentActualAgentState(personId).Should()
				.Be.Null();
		}
	}
}