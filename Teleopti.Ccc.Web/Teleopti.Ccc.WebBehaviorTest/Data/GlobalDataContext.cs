namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public class GlobalDataContext
	{
		private static readonly DataFactory _dataFactory = new DataFactory(GlobalUnitOfWorkState.UnitOfWorkAction);

		public static DataFactory Data() { return _dataFactory; }

		public static void Persist()
		{
			_dataFactory.Apply();
		}
	}
}