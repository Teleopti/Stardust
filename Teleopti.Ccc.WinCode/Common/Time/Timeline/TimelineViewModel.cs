using System;
using System.Windows;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Time.Timeline
{
    public class TimelineViewModel : DependencyObject
    {
        #region fields
        private static readonly DateTimePeriod _defaultPeriod = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
        private static readonly TimeSpan _defaultInterval = TimeSpan.FromHours(1);
        #region dependencyproperties
        public static readonly DependencyProperty PeriodProperty =
            DependencyProperty.Register("Period", typeof(DateTimePeriod), typeof(TimelineViewModel), new UIPropertyMetadata(_defaultPeriod, PeriodPropertyChanged));

        public static readonly DependencyProperty SelectedPeriodProperty =
            DependencyProperty.Register("SelectedPeriod", typeof(DateTimePeriod), typeof(TimelineViewModel), new UIPropertyMetadata(_defaultPeriod, SelectedPeriodChanged, CoerceSelectedPeriod));

        private static object CoerceSelectedPeriod(DependencyObject d, object baseValue)
        {
            DateTimePeriod newPeriod = (DateTimePeriod)baseValue;
            TimelineViewModel model = (TimelineViewModel)d;
            var intersection = model.Period.Intersection(newPeriod);
            return (intersection != null) ? (DateTimePeriod)intersection : model.SelectedPeriod;
        }


        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register("Interval", typeof(TimeSpan), typeof(TimelineViewModel), new UIPropertyMetadata(_defaultInterval, IntervalPropertyChanged));

        #endregion
        #endregion fields

        #region properties

        public DateTimePeriod SelectedPeriod
        {
            get { return (DateTimePeriod)GetValue(SelectedPeriodProperty); }
            set { SetValue(SelectedPeriodProperty, value); }
        }

        public DateTimePeriod Period
        {
            get { return (DateTimePeriod)GetValue(PeriodProperty); }
            set { SetValue(PeriodProperty, value); }
        }

        public TimeSpan Interval
        {
            get { return (TimeSpan)GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }

        public IntervalCollection Intervals
        {
            get;
            private set;
        }

        #region properties for visibility
        public bool ShowLayers
        {
            get { return (bool)GetValue(ShowLayersProperty); }
            set { SetValue(ShowLayersProperty, value); }
        }

        public bool ShowSelectedPeriod
        {
            get { return (bool)GetValue(ShowSelectedPeriodProperty); }
            set { SetValue(ShowSelectedPeriodProperty, value); }
        }
       
        public bool ShowHoverTime
        {
            get { return (bool)GetValue(ShowHoverTimeProperty); }
            set { SetValue(ShowHoverTimeProperty, value); }
        }

        public static readonly DependencyProperty ShowHoverTimeProperty =
            DependencyProperty.Register("ShowHoverTime", typeof(bool), typeof(TimelineViewModel), new UIPropertyMetadata(false));
        public static readonly DependencyProperty ShowSelectedPeriodProperty =
            DependencyProperty.Register("ShowSelectedPeriod", typeof(bool), typeof(TimelineViewModel), new UIPropertyMetadata(false));
        public static readonly DependencyProperty ShowLayersProperty =
            DependencyProperty.Register("ShowLayers", typeof(bool), typeof(TimelineViewModel), new UIPropertyMetadata(false));
        #endregion

        #endregion

        #region ctor
        public TimelineViewModel()
        {
            Intervals = new IntervalCollection(Period, Interval);
        }
        #endregion

        #region private
        private static void PeriodPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DateTimePeriod newPeriod = (DateTimePeriod)e.NewValue;
            TimelineViewModel model = (TimelineViewModel)d;
            model.Intervals.Change(newPeriod);
            model.CoerceValue(SelectedPeriodProperty);
        }

        private static void IntervalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TimelineViewModel)d).Intervals.Change((TimeSpan)e.NewValue);
        }



        private static void SelectedPeriodChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //No need to do anything for now??
        }

        #endregion

    }
}