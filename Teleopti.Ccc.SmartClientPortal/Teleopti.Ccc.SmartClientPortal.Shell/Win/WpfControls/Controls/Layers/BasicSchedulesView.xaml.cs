using System;
using System.Windows;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Layers
{
    /// <summary>
    /// Interaction logic for BasicSchedulesView.xaml
    /// </summary>
    public partial class BasicSchedulesView
    {
        public BasicSchedulesView()
        {
            InitializeComponent();
        }

        private static DateTimePeriod _defaultperiod = new DateTimePeriod(DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)), DateTime.UtcNow.AddDays(1));

        public DateTimePeriod Period
        {
            get { return (DateTimePeriod)GetValue(PeriodProperty); }
            set { SetValue(PeriodProperty, value); }
        }

        public static readonly DependencyProperty PeriodProperty =
            DependencyProperty.Register("Period", typeof(DateTimePeriod), typeof(BasicSchedulesView), new UIPropertyMetadata(_defaultperiod));
    }
}
