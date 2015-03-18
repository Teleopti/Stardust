using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	public class FakeDatabaseConnectionStringHandler : IDatabaseConnectionStringHandler
	{
		public string AppConnectionString(string tenant)
		{
			return ConnectionStringHelper.ConnectionStringUsedInTests;
		}

		public string DataStoreConnectionString(string tenant)
		{
			return ConnectionStringHelper.ConnectionStringUsedInTestsMatrix;
		}
	}
}