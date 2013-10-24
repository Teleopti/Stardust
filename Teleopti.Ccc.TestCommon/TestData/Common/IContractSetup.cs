using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.TestData.Common
{
	public interface IContractSetup : IDataSetup
	{
		IContract Contract { get; }
	}
}