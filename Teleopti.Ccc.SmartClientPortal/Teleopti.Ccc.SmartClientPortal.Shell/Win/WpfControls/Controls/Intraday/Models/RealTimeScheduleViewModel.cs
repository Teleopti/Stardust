using System.ComponentModel;
using System.Windows;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Time.Timeline;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Intraday.Models
{
    public class RealTimeScheduleViewModel:DependencyObject, INotifyPropertyChanged
    {
		private readonly IEventAggregator _eventAggregator;
        private IDayLayerViewModel _dayLayerViewModel;

	    public TimelineControlViewModel TimelineModel { get; private set; }

		public IDayLayerViewModel DayLayerViewModel
        {
            get { return _dayLayerViewModel; }
            private set
            {
	            _dayLayerViewModel = value;
				notifyPropertyChanged(nameof(DayLayerViewModel));
            }
        }

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
		    DependencyProperty.Register("NowPeriod",
		                                typeof (DateTimePeriod),
		                                typeof (RealTimeScheduleViewModel),
		                                new UIPropertyMetadata(new DateTimePeriod(), nowPeriodChanged));


	    public static readonly DependencyProperty PeriodProperty =
		    DependencyProperty.Register("Period",
		                                typeof (DateTimePeriod),
		                                typeof (RealTimeScheduleViewModel),
		                                new UIPropertyMetadata(new DateTimePeriod(), periodChanged));
		
        public RealTimeScheduleViewModel(IEventAggregator eventAggregator,ICreateLayerViewModelService createLayerViewModelService,IDayLayerViewModel dayLayerViewModel)
        {
            _eventAggregator = eventAggregator;
            DayLayerViewModel = dayLayerViewModel;
	        TimelineModel = new TimelineControlViewModel(_eventAggregator, createLayerViewModelService)
		        {
			        ShowHoverTime = false,
			        ShowDate = true,
			        ShowNowPeriod = false,
					ShowNowTime = true
		        };
        }

		private void notifyPropertyChanged(string property)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}

		private static void nowPeriodChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var model = (RealTimeScheduleViewModel)d;
            model.TimelineModel.HoverTime = ((DateTimePeriod)e.NewValue).StartDateTime;
			model.DayLayerViewModel.RefreshElapsedTime(model.NowPeriod.StartDateTime);
        }

        private static void periodChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var model = (RealTimeScheduleViewModel) d;
            var period = (DateTimePeriod) e.NewValue;
            model.TimelineModel.Period = period;
            model.ZoomWidth = (period.ElapsedTime().TotalHours*75);
        }

        //Just for fixing mem leaks in WPF
#pragma warning disable 0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 0067

    }
}
