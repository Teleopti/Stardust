using System.ComponentModel;

namespace Teleopti.Ccc.WinCode.Common.Time
{
    public interface ITimeZoomItemViewModel : INotifyPropertyChanged
    {
        double MinuteWidth { get; }
    }
}
