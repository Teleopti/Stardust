using Teleopti.Interfaces.Domain;
using System.ComponentModel;

namespace Teleopti.Ccc.WinCode.Common.Time
{
    public interface ITimeZoomViewModel:INotifyPropertyChanged
    {
        DateTimePeriod Period { get; }
        double MinuteWidth { get; }
        double PanelWidth { get; }
    }
}
