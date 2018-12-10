using System;
using System.Windows;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Layers
{
    /// <summary>
    /// Interaction logic for LayerEditListBox.xaml
    /// </summary>
    public partial class ExpandedLayersExpandedView
    {
        private static DateTimePeriod _defaultperiod = new DateTimePeriod(DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)), DateTime.UtcNow.AddDays(1));

        public DateTimePeriod Period
        {
            get { return (DateTimePeriod) GetValue(PeriodProperty); }
            set { SetValue(PeriodProperty, value); }
        }

        public static readonly DependencyProperty PeriodProperty =
            DependencyProperty.Register("Period", typeof (DateTimePeriod), typeof (ExpandedLayersExpandedView), new UIPropertyMetadata(_defaultperiod));

        public double Offset
        {
            get { return (double) GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register("Offset", typeof (double), typeof (ExpandedLayersExpandedView), new UIPropertyMetadata(0.0d));

        public bool EditMode
        {
            get { return (bool) GetValue(EditModeProperty); }
            set { SetValue(EditModeProperty, value); }
        }

        public static readonly DependencyProperty EditModeProperty =
            DependencyProperty.Register("EditMode", typeof (bool), typeof (ExpandedLayersExpandedView), new UIPropertyMetadata(false));

        public ExpandedLayersExpandedView()
        {
            InitializeComponent();
        }
    }
}
