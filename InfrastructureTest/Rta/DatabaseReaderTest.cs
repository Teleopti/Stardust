using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[Category("LongRunning")]
	public class DatabaseReaderTest
	{
		[Test]
		public void ShouldGetCurrentActualAgentState()
		{
			var state = new ActualAgentStateForTest { PersonId = Guid.NewGuid() };
			new DatabaseWriter(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler()).PersistActualAgentState(state);
			var target = new DatabaseReader(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler(), new Now());

			var result = target.GetCurrentActualAgentState(state.PersonId);

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldGetCurrentActualAgentStates()
		{
			var writer = new DatabaseWriter(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler());
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			writer.PersistActualAgentState(new ActualAgentStateForTest { PersonId = personId1 });
			writer.PersistActualAgentState(new ActualAgentStateForTest { PersonId = personId2 });
			var target = new DatabaseReader(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler(), new Now());

			var result = target.GetActualAgentStates();

			result.Where(x => x.PersonId == personId1).Should().Have.Count.EqualTo(1);
			result.Where(x => x.PersonId == personId2).Should().Have.Count.EqualTo(1);
		}

	}
}