namespace Teleopti.Ccc.Rta.PerformanceTest.Code
{
	public class TestConfiguration
	{
		public static int HashValue =>
			sourceId.GetHashCode() ^
			dataSourceId ^
			numberOfAgents ^
			numberOfMappings ^ 
			1123532; // change this whenever anything created by DataCreator changes

		private const int numberOfAgents = 2000;
		private const int numberOfMappings = 10000;
		private const string sourceId = "8";
		private const int dataSourceId = 9;

		public int NumberOfAgents => numberOfAgents;
		public int NumberOfMappings => numberOfMappings;

		// this is the switch id
		// and is sent with the state
		public string SourceId => sourceId;

		// this is the analytics data source id
		// and is saved with the external logon in wfm
		public int DataSourceId => dataSourceId;
	}
}