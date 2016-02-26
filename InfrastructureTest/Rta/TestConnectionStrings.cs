using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	public class TestConnectionStrings : IConnectionStrings
	{
		public string Application()
		{
			return InfraTestConfigReader.ConnectionString;
		}

		public string Analytics()
		{
			return InfraTestConfigReader.AnalyticsConnectionString;
		}
	}
}