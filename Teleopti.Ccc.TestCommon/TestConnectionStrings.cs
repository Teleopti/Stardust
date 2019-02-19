using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.TestCommon
{
	public class TestConnectionStrings : IConnectionStrings
	{
		public string Application()
		{
			return InfraTestConfigReader.ApplicationConnectionString();
		}

		public string Analytics()
		{
			return InfraTestConfigReader.AnalyticsConnectionString();
		}
	}
}