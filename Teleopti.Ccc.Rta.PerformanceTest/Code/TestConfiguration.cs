namespace Teleopti.Ccc.Rta.PerformanceTest.Code
{
	public class TestConfiguration
	{
		// change this whenever anything created by DataCreator changes
		public static int HashValue = 3234;

		public int NumberOfAgents = 2;
		public int NumberOfMappings= 2;

		// this is the switch id
		// and is sent with the state
		public string SourceId = "8";

		// this is the analytics data source id
		// and is saved with the external logon in wfm
		public int DataSourceId = 9;
	}
}