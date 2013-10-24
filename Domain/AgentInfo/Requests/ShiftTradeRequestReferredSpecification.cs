using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

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
				var shiftTradeRequest = obj.Request as IShiftTradeRequest;
				if (shiftTradeRequest != null)
					isRefered = shiftTradeRequest.GetShiftTradeStatus(_shiftTradeRequestStatusChecker) == ShiftTradeStatus.Referred;
			}
			return isRefered;
		}
	}
}