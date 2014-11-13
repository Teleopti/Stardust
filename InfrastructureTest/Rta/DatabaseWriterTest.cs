using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[Category("LongRunning")]
	public class DatabaseWriterTest : IActualAgentStateReadWriteTest
	{
		[Test]
		public void ShouldPersistActualAgentState()
		{
			var state = new ActualAgentStateForTest();
			var target = new DatabaseWriter(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler());

			target.PersistActualAgentState(new ActualAgentStateForTest());

			var result = new DatabaseReader(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler(), new Now()).GetCurrentActualAgentState(state.PersonId);
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistActualAgentStateWithNullSourceId()
		{
			var state = new ActualAgentStateForTest();
			var target = new DatabaseWriter(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler());

			target.PersistActualAgentState(new ActualAgentStateForTest { OriginalDataSourceId = null });

			var result = new DatabaseReader(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler(), new Now()).GetCurrentActualAgentState(state.PersonId);
			result.Should().Not.Be.Null();
		}

	}
}