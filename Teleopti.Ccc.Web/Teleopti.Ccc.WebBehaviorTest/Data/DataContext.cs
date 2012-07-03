namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public class DataContext
	{
		private static readonly DataFactory _dataFactory = new DataFactory();

		public static DataFactory Data() { return _dataFactory; }
	}
}