using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public interface IContractSetup : IDataSetup
	{
		IContract Contract { get; }
	}
}