using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class ShiftTradeRequestOkByMeSpecification : Specification<IPersonRequest>
	{
		private readonly IShiftTradeRequestStatusChecker _shiftTradeRequestStatusChecker;

		public ShiftTradeRequestOkByMeSpecification(IShiftTradeRequestStatusChecker shiftTradeRequestStatusChecker)
		{
			_shiftTradeRequestStatusChecker = shiftTradeRequestStatusChecker;
		}

		public override bool IsSatisfiedBy(IPersonRequest obj)
		{
			var isOkByMe = false;
			if (obj != null)
			{
				var shiftTradeRequest = obj.Request as IShiftTradeRequest;
				if (shiftTradeRequest != null)
					isOkByMe = shiftTradeRequest.GetShiftTradeStatus(_shiftTradeRequestStatusChecker) == ShiftTradeStatus.OkByMe;
			}
			return isOkByMe;
		}
	}
}
