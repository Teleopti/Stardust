using Teleopti.Ccc.WinCode.Intraday;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WpfControls.Controls.Intraday
{
    public interface IIntradaySchedulesControl
    {
        void SetDayLayerViewModel(IDayLayerViewModel dayLayerViewModel);
        void RefreshProjection(IPerson person);
    }
}
