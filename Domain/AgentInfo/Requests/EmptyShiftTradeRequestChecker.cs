using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	internal class EmptyShiftTradeRequestChecker : IShiftTradeRequestStatusChecker
	{
		public void Check(IShiftTradeRequest shiftTradeRequest)
		{
		}
	}
}