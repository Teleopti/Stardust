using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default
{
	public interface IHashableDataSetup : IDataSetup
	{
		int HashValue();
	}
}