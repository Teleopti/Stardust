using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class EmptyShiftTradeRequestChecker : IShiftTradeRequestStatusChecker
	{
		public void Check(IShiftTradeRequest shiftTradeRequest)
		{
		}
	}
}