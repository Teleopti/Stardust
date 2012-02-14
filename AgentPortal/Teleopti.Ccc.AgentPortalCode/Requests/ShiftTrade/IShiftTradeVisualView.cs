using System.Collections.Generic;

namespace Teleopti.Ccc.AgentPortalCode.Requests.ShiftTrade
{
    public interface IShiftTradeVisualView
    {
        void SetDataSource(IList<ShiftTradeDetailModel> shiftTradeDetailModels);
        void OnRefresh();
    }
}