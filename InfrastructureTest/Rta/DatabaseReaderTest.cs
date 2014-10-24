using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.TestCommon;
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
			var state = new ActualAgentState {PersonId = Guid.NewGuid(), ReceivedTime = DateTime.UtcNow, OriginalDataSourceId = "0"};
			new DatabaseWriter(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler()).PersistActualAgentState(state);
			var target = new DatabaseReader(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler(), new Now());

			var result = target.GetCurrentActualAgentState(state.PersonId);

			result.Should().Not.Be.Null();
		}

	}

	public class FakeDatabaseConnectionStringHandler : IDatabaseConnectionStringHandler
	{
		public string AppConnectionString()
		{
			return ConnectionStringHelper.ConnectionStringUsedInTests;
		}

		public string DataStoreConnectionString()
		{
			return ConnectionStringHelper.ConnectionStringUsedInTestsMatrix;
		}
	}
}