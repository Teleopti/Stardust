namespace Teleopti.Ccc.AgentPortalCode.Requests.ShiftTrade
{
    public class ShiftTradeVisualPresenter
    {
        private readonly IShiftTradeVisualView _shiftTradeVisualView;
        private readonly ShiftTradeModel _shiftTradeModel;

        public ShiftTradeVisualPresenter(IShiftTradeVisualView shiftTradeVisualView, ShiftTradeModel shiftTradeModel)
        {
            _shiftTradeVisualView = shiftTradeVisualView;
            _shiftTradeModel = shiftTradeModel;
        }

        public void Initialize()
        {
            _shiftTradeVisualView.SetDataSource(_shiftTradeModel.ShiftTradeDetailModels);
        }

        public void OnRefresh()
        {
            _shiftTradeVisualView.SetDataSource(_shiftTradeModel.ShiftTradeDetailModels);
        }
    }
}
