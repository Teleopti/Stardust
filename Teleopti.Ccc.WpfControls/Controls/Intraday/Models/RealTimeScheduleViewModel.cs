using System.ComponentModel;
using System.Windows;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Time.Timeline;
using Teleopti.Ccc.WinCode.Intraday;

using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WpfControls.Controls.Intraday.Models
{
    /// <summary>
    /// ViewModel for agent-grid
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-06-02
    /// </remarks>
    public class RealTimeScheduleViewModel:DependencyObject, INotifyPropertyChanged
    {
        #region properties

        private IDayLayerViewModel _dayLayerViewModel;
        public IDayLayerViewModel DayLayerViewModel
        {
            get { return _dayLayerViewModel; }
            private set
            {
                _dayLayerViewModel = value;
                var handler = PropertyChanged;
                if (handler!=null)
                {
                    handler.Invoke(this, new PropertyChangedEventArgs("DayLayerViewModel"));
                }
            }
        }

        public TimelineControlViewModel TimelineModel { get; private set; }

        public DateTimePeriod Period
        {
            get { return (DateTimePeriod)GetValue(PeriodProperty); }
            set { SetValue(PeriodProperty, value); }
        }
       
        public DateTimePeriod NowPeriod
        {
            get { return (DateTimePeriod)GetValue(NowPeriodProperty); }
            set { SetValue(NowPeriodProperty, value); }
        }

        public double ZoomWidth
        {
            get { return (double) GetValue(ZoomWidthProperty); }
            set { SetValue(ZoomWidthProperty, value); }
        }

        public static readonly DependencyProperty ZoomWidthProperty =
            DependencyProperty.Register("ZoomWidth", 
            typeof (double), 
            typeof (RealTimeScheduleViewModel), 
            new UIPropertyMetadata(1000d));


        public static readonly DependencyProperty NowPeriodProperty =
            DependencyProperty.Register("NowPeriod", typeof(DateTimePeriod), typeof(RealTimeScheduleViewModel), new UIPropertyMetadata(new DateTimePeriod(),NowPeriodChanged));


        public static readonly DependencyProperty PeriodProperty =
            DependencyProperty.Register("Period", typeof(DateTimePeriod), typeof(RealTimeScheduleViewModel), new UIPropertyMetadata(new DateTimePeriod(),PeriodChanged));

        private IEventAggregator _eventAggregator;

        #endregion

        public RealTimeScheduleViewModel(IEventAggregator eventAggregator,ICreateLayerViewModelService createLayerViewModelService,IDayLayerViewModel dayLayerViewModel)
        {
            _eventAggregator = eventAggregator;
            DayLayerViewModel = dayLayerViewModel;
            TimelineModel = new TimelineControlViewModel(_eventAggregator,createLayerViewModelService);
            TimelineModel.ShowHoverTime = false;
            TimelineModel.ShowDate = true;
            TimelineModel.ShowNowPeriod = true;

        }

        private static void NowPeriodChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var model = (RealTimeScheduleViewModel)d;
            model.TimelineModel.HoverTime = ((DateTimePeriod)e.NewValue).StartDateTime;
			model.DayLayerViewModel.RefreshElapsedTime(model.NowPeriod.StartDateTime);
        }

        private static void PeriodChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RealTimeScheduleViewModel model = (RealTimeScheduleViewModel) d;
            DateTimePeriod period = (DateTimePeriod) e.NewValue;
            model.TimelineModel.Period = period;
            model.ZoomWidth = (period.ElapsedTime().TotalHours*75);
        }
        
        //Just for fixing mem leaks in WPF
#pragma warning disable 0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 0067
    }
}
