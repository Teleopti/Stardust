using System.ComponentModel;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Time
{
    public interface ITimeZoomItemViewModel : INotifyPropertyChanged
    {
        double MinuteWidth { get; }
    }
}
