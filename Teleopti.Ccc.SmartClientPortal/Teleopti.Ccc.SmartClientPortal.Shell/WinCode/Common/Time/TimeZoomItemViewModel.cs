using System.ComponentModel;
using System.Windows;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Time
{
    public class TimeZoomItemViewModel : DependencyObject, ITimeZoomItemViewModel
    {
        public double MinuteWidth
        {
            get { return (double)GetValue(MinuteWidthProperty); }
            set
            {
                SetValue(MinuteWidthProperty, value);
                InvokePropertyChanged(new PropertyChangedEventArgs("MinuteWidth"));
            }
        }

        public static readonly DependencyProperty MinuteWidthProperty =
        DependencyProperty.Register("MinuteWidth",
        typeof(double),
        typeof(TimeZoomItemViewModel),
        new UIPropertyMetadata(1d));

        public TimeZoomItemViewModel(double minuteWidth)
        {
            MinuteWidth = minuteWidth;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }
    }
}
