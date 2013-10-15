using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
    public interface IClearReferredShiftTradeRequests
    {
        void ClearReferredShiftTradeRequests();
    }

    public class ShiftTradeRequestStatusCheckerWithSchedule : IShiftTradeRequestStatusChecker, IClearReferredShiftTradeRequests
    {
        private readonly IScheduleDictionary _scheduleDictionary;
        private readonly IPersonRequestCheckAuthorization _authorization;

        private readonly IDictionary<IShiftTradeRequest, ShiftTradeStatus> _referredShiftTradeRequests =
            new Dictionary<IShiftTradeRequest, ShiftTradeStatus>();

        public ShiftTradeRequestStatusCheckerWithSchedule(IScheduleDictionary scheduleDictionary, IPersonRequestCheckAuthorization authorization)
        {
            _scheduleDictionary = scheduleDictionary;
            _authorization = authorization;
        }

        public void Check(IShiftTradeRequest shiftTradeRequest)
        {
            if (shiftTradeRequest == null) return;
            if (shiftTradeRequest.Parent==null) return;

            var parentRequest = (IPersonRequest) shiftTradeRequest.Parent;
            if (!(parentRequest.IsPending || parentRequest.IsNew)) return;

            var requestStatusChecker = new emptyShiftTradeRequestChecker();
            var previousStatus = shiftTradeRequest.GetShiftTradeStatus(requestStatusChecker);
            if (!ShiftTradeRequestStatusChecker.VerifyShiftTradeIsUnchanged(_scheduleDictionary, shiftTradeRequest,_authorization))
            {
	            if (_referredShiftTradeRequests.ContainsKey(shiftTradeRequest))
		            _referredShiftTradeRequests[shiftTradeRequest] = previousStatus;

	            else
		            _referredShiftTradeRequests.Add(shiftTradeRequest, previousStatus);
            }
            else
            {
                ShiftTradeStatus shiftTradeStatus;
                if (_referredShiftTradeRequests.TryGetValue(shiftTradeRequest,out shiftTradeStatus))
                {
                    _referredShiftTradeRequests.Remove(shiftTradeRequest);
                    if (shiftTradeStatus == ShiftTradeStatus.OkByBothParts || shiftTradeStatus==ShiftTradeStatus.Referred)
                        shiftTradeRequest.SetShiftTradeStatus(ShiftTradeStatus.OkByMe,_authorization);

                    shiftTradeRequest.SetShiftTradeStatus(ShiftTradeStatus.OkByBothParts,_authorization);
                }

            }
        }

        public void ClearReferredShiftTradeRequests()
        {
            _referredShiftTradeRequests.Clear();
        }

        private class emptyShiftTradeRequestChecker : IShiftTradeRequestStatusChecker
        {
            public void Check(IShiftTradeRequest shiftTradeRequest)
            {
            }
        }
    }
}