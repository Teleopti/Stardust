using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	public class TestConnectionStrings : IConnectionStrings
	{
		public string Application()
		{
			return ConnectionStringHelper.ConnectionStringUsedInTests;
		}

		public string Analytics()
		{
			return ConnectionStringHelper.ConnectionStringUsedInTestsMatrix;
		}
	}
}