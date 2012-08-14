namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public class GlobalDataContext
	{
		private static readonly DataFactory _dataFactory = new DataFactory();

		public static DataFactory Data() { return _dataFactory; }

		public static void Persist()
		{
			GlobalUnitOfWorkState.UnitOfWorkAction(uow => _dataFactory.Apply(uow));
		}
	}
}