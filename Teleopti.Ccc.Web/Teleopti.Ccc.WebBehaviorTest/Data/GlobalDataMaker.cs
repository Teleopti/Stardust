using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public class GlobalDataMaker
	{
		private static readonly DataFactory _dataFactory = new DataFactory(GlobalUnitOfWorkState.UnitOfWorkAction);

		public static DataFactory Data() { return _dataFactory; }

	}
}