using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public static class ContractExtensions
	{
		public static IContract WithNoDayOffTolerance(this IContract contract)
		{
			contract.NegativeDayOffTolerance = 0;
			contract.PositiveDayOffTolerance = 0;
			return contract;
		}
	}
}