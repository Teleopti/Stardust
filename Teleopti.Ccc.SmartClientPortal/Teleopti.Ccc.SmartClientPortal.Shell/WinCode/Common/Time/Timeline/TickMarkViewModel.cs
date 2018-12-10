using System;
using System.ComponentModel;
using System.Windows;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Time.Timeline
{
    public class TickMarkViewModel : FrameworkElement, INotifyPropertyChanged
    {
        private DateTimePeriod _period;

        public double MinorTickMarkHeight
        {
            get { return (double)GetValue(MinorTickMarkHeightProperty); }
            set { SetValue(MinorTickMarkHeightProperty, value); }
        }

        public static readonly DependencyProperty MinorTickMarkHeightProperty =
            DependencyProperty.Register("MinorTickMarkHeight", typeof(double), typeof(TickMarkViewModel), new UIPropertyMetadata(10d));

        public double MinimumTimeHeight
        {
            get { return (double)GetValue(MinimumTimeHeightProperty); }
            set { SetValue(MinimumTimeHeightProperty, value); }
        }

        public static readonly DependencyProperty MinimumTimeHeightProperty =
            DependencyProperty.Register("MinimumTimeHeight", typeof(double), typeof(TickMarkViewModel), new UIPropertyMetadata(20d));

        public double MajorTickMarkHeight
        {
            get { return (double)GetValue(MajorTickMarkHeightProperty); }
            set { SetValue(MajorTickMarkHeightProperty, value); }
        }

        public static readonly DependencyProperty MajorTickMarkHeightProperty =
            DependencyProperty.Register("MajorTickMarkHeight", typeof(double), typeof(TickMarkViewModel), new UIPropertyMetadata(13d));

        public bool ShowTickMarks
        {
            get { return (bool)GetValue(ShowTickMarksProperty); }
            set { SetValue(ShowTickMarksProperty, value); }
        }


        public static readonly DependencyProperty ShowTickMarksProperty =
            DependencyProperty.Register("ShowTickMarks", typeof(bool), typeof(TickMarkViewModel), new UIPropertyMetadata(true));

        public TimeSpan Resolution
        {
            get { return (TimeSpan)GetValue(ResolutionProperty); }
            set { SetValue(ResolutionProperty, value); }
        }


        public static readonly DependencyProperty ResolutionProperty =
            DependencyProperty.Register("Resolution", typeof(TimeSpan), typeof(TickMarkViewModel), new FrameworkPropertyMetadata(TimeSpan.FromHours(1)));

        public TickMarkViewModel(DateTimePeriod period, TimeSpan resolution)
        {
            Period = period;
            Resolution = resolution;
        }

        public DateTimePeriod Period
        {
            get { return _period; }
            set
            {
                if (_period != value)
                {
                    _period = value;

                	var handler = PropertyChanged;
                    if (handler != null) PropertyChanged(this, new PropertyChangedEventArgs("Period"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}