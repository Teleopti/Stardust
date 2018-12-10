using System;
using System.Windows;
using System.Windows.Controls;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Layers
{
    /// <summary>
    /// Interaction logic for ExpandedLayersView_Pre.xaml
    /// </summary>
    public partial class ExpandedLayersExpandedPreview : UserControl
    {
        private static DateTimePeriod _defaultperiod = new DateTimePeriod(DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)), DateTime.UtcNow.AddDays(1));

        public DateTimePeriod Period
        {
            get { return (DateTimePeriod) GetValue(PeriodProperty); }
            set { SetValue(PeriodProperty, value); }
        }

        public static readonly DependencyProperty PeriodProperty =
            DependencyProperty.Register("Period", typeof (DateTimePeriod), typeof (ExpandedLayersExpandedPreview), new UIPropertyMetadata(_defaultperiod));

        public double Offset
        {
            get { return (double) GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register("Offset", typeof (double), typeof (ExpandedLayersExpandedPreview), new UIPropertyMetadata(0.0d));

        public bool EditMode
        {
            get { return (bool) GetValue(EditModeProperty); }
            set { SetValue(EditModeProperty, value); }
        }

        public static readonly DependencyProperty EditModeProperty =
            DependencyProperty.Register("EditMode", typeof (bool), typeof (ExpandedLayersExpandedPreview), new UIPropertyMetadata(false));

        public ExpandedLayersExpandedPreview()
        {
            InitializeComponent();
        }
    }
}
