using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Intraday
{
    public interface IIntradaySchedulesControl
    {
        void SetDayLayerViewModel(IDayLayerViewModel dayLayerViewModel);
        void RefreshProjection(IPerson person);
    }
}
