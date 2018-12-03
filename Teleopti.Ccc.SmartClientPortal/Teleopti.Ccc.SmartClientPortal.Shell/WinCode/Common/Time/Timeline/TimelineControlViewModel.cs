using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Time.Timeline
{
    /// <summary>
    /// Model for timeline
    /// </summary>
    /// <remarks>
    /// Note: Will be replaced with TimlineViewModel
    /// Created by: henrika
    /// Created date: 2009-09-01
    /// </remarks>
    public class TimelineControlViewModel : DependencyObject, INotifyPropertyChanged
    {
        public TimelineControlViewModel(IEventAggregator eventAggregator,ICreateLayerViewModelService createLayerViewModelService)
        {
            _layers = new LayerViewModelCollection(eventAggregator, createLayerViewModelService,new RemoveLayerFromSchedule(), new ReplaceLayerInSchedule(), PrincipalAuthorization.Current());
            CurrentDispatcher = Dispatcher.CurrentDispatcher;
            TimeZoom = new TimeZoomViewModel(Period);
            TickMarks = new ObservableCollection<TickMarkViewModel>();
            TickMarkDays = new ObservableCollection<DateTimePeriod>();
        }

        public IList<TickMarkViewModel> TickMarks { get; private set; }
        public ObservableCollection<DateTimePeriod> TickMarkDays { get; private set; }

        public DispatcherTimer NowTimer { get; private set; }
        public Dispatcher CurrentDispatcher { get; private set; }

        public DateTimePeriod NowPeriod
        {
            get { return (DateTimePeriod)GetValue(NowPeriodProperty); }
            set { SetValue(NowPeriodProperty, value); }
        }

        public static readonly DependencyProperty NowPeriodProperty =
            DependencyProperty.Register("NowPeriod",
            typeof(DateTimePeriod),
            typeof(TimelineControlViewModel),
            new UIPropertyMetadata(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)))));

        public bool ShowNowPeriod
        {
            get { return (bool)GetValue(ShowNowPeriodProperty); }
            set { SetValue(ShowNowPeriodProperty, value); }
        }

        public static readonly DependencyProperty ShowNowPeriodProperty =
            DependencyProperty.Register("ShowNowPeriod", typeof(bool), typeof(TimelineControlViewModel), new UIPropertyMetadata(false, ShowNowPeriodChanged));

        public bool ShowNowTime
        {
            get { return false; }
            set { SetValue(ShowNowTimeProperty, value); }
        }

        public bool ShowDate
        {
            get { return (bool)GetValue(ShowDateProperty); }
            set { SetValue(ShowDateProperty, value); }
        }

        public static readonly DependencyProperty ShowDateProperty =
          DependencyProperty.Register("ShowDate", typeof(bool), typeof(TimelineControlViewModel), new UIPropertyMetadata(false));

        public TimeSpan HoverWidth
        {
            get { return (TimeSpan)GetValue(HoverWidthProperty); }
            set { SetValue(HoverWidthProperty, value); }
        }

        public static readonly DependencyProperty HoverWidthProperty =
            DependencyProperty.Register("HoverWidth", typeof(TimeSpan), typeof(TimelineControlViewModel), new UIPropertyMetadata(TimeSpan.FromMinutes(1), HoverWidthChanged));

        private static void HoverWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TimelineControlViewModel)d).NotifyPropertyChanged(nameof(HoverPeriod));
        }

        public DateTimePeriod HoverPeriod
        {
            get { return new DateTimePeriod(HoverTime, HoverTime.Add(HoverWidth)); }
        }
        
        public DateTime HoverTime
        {
            get { return _hoverTime; }
            set
            {
                if (_hoverTime != value)
                {
                    _hoverTime = value;
                    NotifyPropertyChanged(nameof(HoverTime));
                    NotifyPropertyChanged(nameof(HoverPeriod));
                }
            }
        }
        public bool ShowHoverTime
        {
            get { return _showHoverTime; }
            set
            {
                if (_showHoverTime != value)
                {
                    _showHoverTime = value;
                    NotifyPropertyChanged(nameof(ShowHoverTime));
                }
            }
        }

	    public double NowTimeOpacity
	    {
			get { return _nowTimeOpacity; }
		    set
		    {
			    _nowTimeOpacity = value;
				NotifyPropertyChanged(nameof(NowTimeOpacity));
		    }
	    }

        public DateTimePeriod SelectedPeriod
        {
            get { return _selecteDateTimePeriod; }
            set
            {
                if (value != _selecteDateTimePeriod)
                {
                    _selecteDateTimePeriod = value;
                    NotifyPropertyChanged(nameof(SelectedPeriod));
                }
            }
        }

        public bool ShowSelectedPeriod
        {
            get { return _showSelectedPeriod; }
            set
            {
                if (_showSelectedPeriod != value)
                {
                    _showSelectedPeriod = value;
                    NotifyPropertyChanged(nameof(ShowSelectedPeriod));
                }
            }
        }

        public bool ShowTickMark
        {
            get { return (bool)GetValue(ShowTickMarkProperty); }
            set { SetValue(ShowTickMarkProperty, value); }
        }

        public static readonly DependencyProperty ShowTickMarkProperty =
            DependencyProperty.Register("ShowTickMark",
                                        typeof(bool),
                                        typeof(TimelineControlViewModel),
                                        new FrameworkPropertyMetadata(true));

        public LayerViewModelCollection Layers
        {
            get { return _layers; }
        }
        public bool ShowLayers
        {
            get { return _showLayers; }
            set
            {
                if (_showLayers != value)
                {
                    _showLayers = value;
                    NotifyPropertyChanged(nameof(ShowLayers));
                }
            }
        }

        public static readonly DependencyProperty ShowNowTimeProperty =
        DependencyProperty.Register("ShowNowTime", typeof(bool), typeof(TimelineControlViewModel), new UIPropertyMetadata(false));

        public DateTimePeriod Period
        {
            get { return (DateTimePeriod)GetValue(PeriodProperty); }
            set
            {
                SetValue(PeriodProperty, value);
            }
        }

        public static readonly DependencyProperty PeriodProperty =
            DependencyProperty.Register("Period",
            typeof(DateTimePeriod),
            typeof(TimelineControlViewModel),
            new FrameworkPropertyMetadata(new DateTimePeriod(DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc),
                DateTime.SpecifyKind(DateTime.MinValue.AddMinutes(1), DateTimeKind.Utc)), FrameworkPropertyMetadataOptions.AffectsParentArrange, PeriodChanged));

        private static void PeriodChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //Set Tickmarks:
            //Henrik 20090703 todo: Refactor out the omx-if-resolution-thing
            var model = d as TimelineControlViewModel;
            if (model != null)
            {
                model.TickMarks.Clear();
                model.TickMarkDays.Clear();
                IList<DateTimePeriod> list;
                if (model.Resolution == TimeSpan.FromDays(1))
                    list = model.Period.WholeDayCollection(model.TimeZone );
                else
                    list = EvenHourPeriodsInCurrentTimeZone(model.Period, model.TimeZone);
                foreach (DateTimePeriod period in list)
                {
                    model.TickMarks.Add(new TickMarkViewModel(period, model.Resolution));
                    //this MUST be refactored
                    if (period.StartDateTime.Hour == 0)
                    {
                        model.TickMarkDays.Add(new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddDays(1)));
                    }
                }
                model.NotifyPropertyChanged(nameof(TickMarks));
                model.TimeZoom.Period = (DateTimePeriod)e.NewValue;
            }
        }

        public TimeSpan Resolution
        {
            get { return (TimeSpan)GetValue(ResolutionProperty); }
            set { SetValue(ResolutionProperty, value); }
        }

        public static readonly DependencyProperty ResolutionProperty =
            DependencyProperty.Register("Resolution", typeof(TimeSpan), typeof(TimelineControlViewModel), new FrameworkPropertyMetadata(TimeSpan.FromHours(1)));

        public TimeSpan Interval
        {
            get { return (TimeSpan)GetValue(IntervalProperty); }
            set
            {
                SetValue(IntervalProperty, value);
            }
        }

        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register("Interval",
                typeof(TimeSpan),
                typeof(TimelineControlViewModel),
                new FrameworkPropertyMetadata(TimeSpan.FromMinutes(15)));

        public TimeZoomViewModel TimeZoom { get; private set; }

        /// <summary>
        /// Gets or sets the time zone for calculating complete hours
        /// </summary>
        /// <value>The time zone.</value>
        public TimeZoneInfo TimeZone
        {
            get { return (TimeZoneInfo)GetValue(TimeZoneProperty); }
            set { SetValue(TimeZoneProperty, value); }
        }

        public static readonly DependencyProperty TimeZoneProperty =
            DependencyProperty.Register("TimeZone", typeof(TimeZoneInfo), typeof(TimelineControlViewModel), new UIPropertyMetadata(TimeZoneInfo.Local));

        private readonly LayerViewModelCollection _layers;
        private DateTimePeriod _selecteDateTimePeriod;
       
        private DateTime _hoverTime = DateTime.UtcNow;
        private bool _showLayers;
        private bool _showSelectedPeriod;
        private bool _showHoverTime;
	    private double _nowTimeOpacity = 1;

	    private static IList<DateTimePeriod> EvenHourPeriodsInCurrentTimeZone(DateTimePeriod period, TimeZoneInfo info)
        {
            //We must check from the Gui if the timezone has different minutes:
            //Henrik 2009-04-01 This needs some refactoring and rethinking
            IList<DateTimePeriod> ret = new List<DateTimePeriod>();
            int diffInMinutes = info.BaseUtcOffset.Minutes;

            foreach (DateTimePeriod d in period.AffectedHourCollection())
            {
                DateTimePeriod hour = d.MovePeriod(TimeSpan.FromMinutes(diffInMinutes));
                ret.Add(hour);
            }
            return ret;
        }

        private static void ShowNowPeriodChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimelineControlViewModel model = (TimelineControlViewModel)d;
            bool show = (bool)e.NewValue;
            if (show)
            {
                if (model.NowTimer == null)
                {
                    WeakReference weakReference = new WeakReference(new EventHandler(model.NowTimerTick));
                    model.NowTimer = new DispatcherTimer(TimeSpan.FromMinutes(1), DispatcherPriority.Render, (EventHandler) weakReference.Target, model.CurrentDispatcher);
                }
                model.NowTimer.Start();
                model.ChangeNowTime();
            }
            else
            {
                if (model.NowTimer != null)
                {
                    model.NowTimer.Stop();
                    model.NowTimer = null;
                }
            }
        }

        private void NowTimerTick(object sender, EventArgs e)
        {
            ChangeNowTime();
        }

        public void ChangeNowTime()
        {
            if (Period.Contains(DateTime.UtcNow))
            {
                NowPeriod = new DateTimePeriod(Period.StartDateTime, DateTime.UtcNow);
            }
            else NowPeriod = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string property)
        {
        	var handler = PropertyChanged;
            if (handler!= null)
            {
            	handler.Invoke(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
