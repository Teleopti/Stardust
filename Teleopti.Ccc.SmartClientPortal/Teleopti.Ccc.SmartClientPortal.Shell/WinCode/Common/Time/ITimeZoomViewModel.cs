using System.ComponentModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Time
{
    public interface ITimeZoomViewModel:INotifyPropertyChanged
    {
        DateTimePeriod Period { get; }
        double MinuteWidth { get; }
        double PanelWidth { get; }
    }
}
