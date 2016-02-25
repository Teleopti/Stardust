using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Default
{
	public interface IHashableDataSetup : IDataSetup
	{
		int HashValue();
	}
}