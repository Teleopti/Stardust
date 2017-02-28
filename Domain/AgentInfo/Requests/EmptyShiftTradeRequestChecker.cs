using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class EmptyShiftTradeRequestChecker : IShiftTradeRequestStatusChecker
	{
		public void Check(IShiftTradeRequest shiftTradeRequest)
		{
		}
	}
}