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
            bool isRefered = false;
            if (obj != null)
            {
                IShiftTradeRequest shiftTradeRequest = obj.Request as IShiftTradeRequest;
                if (shiftTradeRequest != null)
                    isRefered = shiftTradeRequest.GetShiftTradeStatus(_shiftTradeRequestStatusChecker) == ShiftTradeStatus.OkByMe;
            }
            return isRefered;
        }
    }
}
