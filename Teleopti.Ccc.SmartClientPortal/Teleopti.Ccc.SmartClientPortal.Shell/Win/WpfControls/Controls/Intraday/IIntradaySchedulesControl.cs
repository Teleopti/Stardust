using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Intraday;

namespace Teleopti.Ccc.Win.WpfControls.Controls.Intraday
{
    public interface IIntradaySchedulesControl
    {
        void SetDayLayerViewModel(IDayLayerViewModel dayLayerViewModel);
        void RefreshProjection(IPerson person);
    }
}
