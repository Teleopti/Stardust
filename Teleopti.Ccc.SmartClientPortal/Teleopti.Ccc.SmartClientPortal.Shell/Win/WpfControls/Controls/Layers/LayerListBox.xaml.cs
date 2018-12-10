using System;
using System.Windows;
using System.Windows.Controls;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Layers
{
    /// <summary>
    /// For showing a LayerViewModelCollection in a simple time-based way
    /// </summary>
    /// <remarks>
    /// Use for just showing layers in a easy way, not intended to be super-customizable
    /// </remarks>
    public partial class LayerListBox : ListBox
    {
        private static readonly DateTimePeriod Defaultperiod = new DateTimePeriod(DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)), DateTime.UtcNow.AddDays(1));

        public DateTimePeriod Period
        {
            get { return (DateTimePeriod) GetValue(PeriodProperty); }
            set { SetValue(PeriodProperty, value); }
        }

        public static readonly DependencyProperty PeriodProperty =
            DependencyProperty.Register("Period", typeof (DateTimePeriod), typeof (LayerListBox), new UIPropertyMetadata(Defaultperiod));

        public double Offset
        {
            get { return (double) GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register("Offset", typeof (double), typeof (LayerListBox), new UIPropertyMetadata(0.0d));

        public bool EditMode
        {
            get { return (bool) GetValue(EditModeProperty); }
            set { SetValue(EditModeProperty, value); }
        }

        public static readonly DependencyProperty EditModeProperty =
            DependencyProperty.Register("EditMode", typeof (bool), typeof (LayerListBox), new UIPropertyMetadata(false));

        public LayerListBox()
        {
            InitializeComponent();
        }
    }
}
