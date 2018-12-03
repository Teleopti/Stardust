using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Time.Timeline;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Time.TimePanel
{

    /// <summary>
    /// Simple control for presenting timeline
    /// Presents a timeline that can mark a period, not hit visible
    /// </summary>
    /// <remarks>
    /// Wraps a timelineviewmodel, add the properties to be exposed
    /// Created by: henrika
    /// Created date: 2009-06-02
    /// </remarks>
    public partial class SimpleTimePanel: UserControl
    {
        private TimelineControlViewModel _model;

        public SimpleTimePanel(IEventAggregator eventAggregator)
        {
            _model = new TimelineControlViewModel(eventAggregator,new CreateLayerViewModelService());
        }

        public DateTimePeriod Period
        {
            get { return (DateTimePeriod)GetValue(PeriodProperty); }
            set { SetValue(PeriodProperty, value); }
        }

        public static readonly DependencyProperty PeriodProperty =
            DependencyProperty.Register("Period", typeof(DateTimePeriod), typeof(SimpleTimePanel), new UIPropertyMetadata(new DateTimePeriod(),PeriodChanged));

        public DateTimePeriod MarkedPeriod
        {
            get { return (DateTimePeriod)GetValue(MarkedPeriodProperty); }
            set { SetValue(MarkedPeriodProperty, value); }
        }

        public static readonly DependencyProperty MarkedPeriodProperty =
            DependencyProperty.Register("MarkedPeriod", typeof(DateTimePeriod), typeof(SimpleTimePanel), new UIPropertyMetadata(new DateTimePeriod(),MarkedPeriodChanged));

        public bool ShowMark
        {
            get { return (bool)GetValue(ShowMarkProperty); }
            set { SetValue(ShowMarkProperty, value); }
        }



        public bool ShowTickMarks
        {
            get { return (bool)GetValue(ShowTickMarksProperty); }
            set { SetValue(ShowTickMarksProperty, value); }
        }

        public static readonly DependencyProperty ShowTickMarksProperty =
            DependencyProperty.Register("ShowTickMarks", typeof(bool), typeof(SimpleTimePanel), new UIPropertyMetadata(false,ShowTickMarksChanged));

        public static readonly DependencyProperty ShowMarkProperty =
            DependencyProperty.Register("ShowMark", typeof(bool), typeof(SimpleTimePanel), new UIPropertyMetadata(false,ShowMarkedChanged));


        public SimpleTimePanel()
        {
            InitializeComponent();
            _model.ShowTickMark = true;
            DataContext = _model;

        }
       

        private static void ShowMarkedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GetModel(d).ShowTickMark = (bool)e.NewValue;
        }

        private static void PeriodChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GetModel(d).Period = (DateTimePeriod)e.NewValue;
        }

        private static void MarkedPeriodChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GetModel(d).HoverTime = ((DateTimePeriod)e.NewValue).StartDateTime;
                        
        }
        private static void ShowTickMarksChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GetModel(d).ShowTickMark = ((bool)e.NewValue);
        }

        private static TimelineControlViewModel GetModel(DependencyObject d)
        {
            return ((SimpleTimePanel)d)._model;
        }

    }
}
