namespace Teleopti.Ccc.Rta.PerformanceTest.Code
{
	public class TestConfiguration
	{
		public static int HashValue =>
			sourceId.GetHashCode() ^
			dataSourceId ^
			numberOfAgentsInSystem ^
			numberOfAgentsWorking ^
			numberOfMappings ^
			345452; // change this whenever anything created by DataCreator changes

		private const int numberOfAgentsInSystem = 10000;
		private const int numberOfAgentsWorking = 2000;
		private const int numberOfMappings = 10000;
		private const string sourceId = "8";
		private const int dataSourceId = 9;

		public int NumberOfAgentsInSystem => numberOfAgentsInSystem;
		public int NumberOfAgentsWorking => numberOfAgentsWorking;
		public int NumberOfMappings => numberOfMappings;

		// this is the switch id
		// and is sent with the state
		public string SourceId => sourceId;

		// this is the analytics data source id
		// and is saved with the external logon in wfm
		public int DataSourceId => dataSourceId;
	}
}