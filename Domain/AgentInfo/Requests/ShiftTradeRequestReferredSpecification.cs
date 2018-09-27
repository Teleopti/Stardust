using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class ShiftTradeRequestReferredSpecification : Specification<IPersonRequest>
	{
		private readonly IShiftTradeRequestStatusChecker _shiftTradeRequestStatusChecker;

		public ShiftTradeRequestReferredSpecification(IShiftTradeRequestStatusChecker shiftTradeRequestStatusChecker)
		{
			_shiftTradeRequestStatusChecker = shiftTradeRequestStatusChecker;
		}

		public override bool IsSatisfiedBy(IPersonRequest obj)
		{
			var isRefered = false;
			if (obj != null)
			{
				if (obj.Request is IShiftTradeRequest shiftTradeRequest)
					isRefered = shiftTradeRequest.GetShiftTradeStatus(_shiftTradeRequestStatusChecker) == ShiftTradeStatus.Referred;
			}
			return isRefered;
		}
	}
}