using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Time;

using DateTimePeriod = Teleopti.Interfaces.Domain.DateTimePeriod;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{

    /// <summary>
    /// Class for holding a DateTimeperiod for DataBinding (Winform and WPF)
    /// It will always keep the period correct by having a minimum interval between start & end 
    /// It will always be : MIN less than  START + INTERVAL less than  END less than MAX
    /// When changing the Start:
    ///     *If less than Min, it will be set to min
    ///     *If larger than End - interval, End will be coerced to min + interval
    /// When changing the End:
    ///     *If larger than Max, the End will be set to Max
    ///     *If less than Start - Interval, Start will be coerced to End - Interval
    /// </summary>
    public class DateTimePeriodViewModel : DependencyObject, INotifyPropertyChanged,IDataErrorInfo,IViewModelIsValid
    {
        #region Fields & Properties

        private MinMax<DateTime> _minMax;
        private Domain.InterfaceLegacy.Domain.DateTimePeriod _dateTimePeriod;
        private bool _sameDateUtc;
        private bool _sameDateLocal;
        private ObservableCollection<String> _selectableTimeSpans = new ObservableCollection<String>();
        public static readonly TimeSpan MinimumInterval = TimeSpan.FromMinutes(5); //This is the smallest timespanDiff.
        public static readonly TimeSpan MaximumInterval = TimeSpan.FromHours(1); //This is the smallest timespanDiff.
        private bool _autoUpdate = true;
        private bool _isValid = true; //change this to use dep. property



        /// <summary>
        /// Gets a collection of predifined TimeSpans  for the StartTime
        /// </summary>
        /// <value>The selectable start times.</value>
        /// <remarks>
        /// Like 00:00,00:15,00:30 etc
        /// Created by: henrika
        /// Created date: 2010-07-14
        /// </remarks>
        public ListCollectionView SelectableStartTimes { get; private set; }

        /// <summary>
        /// Gets a collection of predifined TimeSpans  for the EndTime
        /// </summary>
        /// <value>The selectable start times.</value>
        /// <remarks>
        /// Like 00:00,00:15,00:30 etc
        /// Created by: henrika
        /// Created date: 2010-07-14
        /// </remarks>
        public ListCollectionView SelectableEndTimes { get; private set; }

        public bool AutoUpdate
        {
            get { return _autoUpdate; }
            set
            {
                if (_autoUpdate != value)
                {
                    _autoUpdate = value;
                    if (AutoUpdate && !IsValid) End = Start.Add(Interval);
                    NotifyPropertyChanged(nameof(AutoUpdate));
                }
            }
        }


        public Domain.InterfaceLegacy.Domain.DateTimePeriod DateTimePeriod
        {
            get { return _dateTimePeriod; }
            private set
            {
                if (value != _dateTimePeriod)
                {
                    _dateTimePeriod = value;
                    SameDateLocal = StartDateTimeAsLocal.Date == EndDateTimeAsLocal.Date;
                    SameDateUtc = Start.Date == End.Date;
                    NotifyPropertyChanged(nameof(DateTimePeriod));
                }
            }
        }

        public MinMax<DateTime> MinMax
        {
            get { return _minMax; }
            private set
            {
                if (value != _minMax)
                {
                    _minMax = value;
                    NotifyPropertyChanged(nameof(MinMax));
                }
            }
        }

        public DateTime Min
        {
            get { return (DateTime)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }

        public DateTime Max
        {
            get { return (DateTime)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }
        public DateTime End
        {
            get { return (DateTime)GetValue(EndProperty); }
            set
            {
                SetValue(EndProperty, value);
                NotifyPropertyChanged(nameof(End));
            }
        }

        public DateTime Start
        {
            get { return (DateTime)GetValue(StartProperty); }
            set
            {
                SetValue(StartProperty, value);
                NotifyPropertyChanged(nameof(Start));
            }
        }

        public TimeSpan Interval
        {
            get { return (TimeSpan)GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }

        public DateTimeViewModel StartDateTimeViewModel { get; private set; }

        public DateTimeViewModel EndDateTimeViewModel { get; private set; }

        public CommandModel MoveStartTimeEarlierCommand { get; private set; }
        public CommandModel MoveStartTimeLaterCommand { get; private set; }
        public CommandModel MoveEndTimeEarlierCommand { get; private set; }
        public CommandModel MoveEndTimeLaterCommand { get; private set; }
        public CommandModel MovePeriodEarlierCommand { get; private set; }
        public CommandModel MovePeriodLaterCommand { get; private set; }

        /// <summary>
        /// Sets/Resets the AutoUpdate on the Period
        /// </summary>
        /// <value>The toggle auto update period.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2010-09-01
        /// </remarks>
        public CommandModel ToggleAutoUpdatePeriod
        {
            get;
            private set;
        }

        #endregion //Fields & Properties

        #region DependencyProperties

        #region Registrations

        //note: might set the default value to MinimumInterval?
        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register("Interval",
            typeof(TimeSpan),
            typeof(DateTimePeriodViewModel),
            new UIPropertyMetadata(TimeSpan.Zero,
                IntervalChanged));

        public static readonly DependencyProperty MinProperty =
            DependencyProperty.Register("Min",
            typeof(DateTime),
            typeof(DateTimePeriodViewModel),
            new UIPropertyMetadata(DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc),
                MinPropertyChanged,
                CoerceMinValue),
            delegate(object value)
            {
                InParameter.VerifyDateIsUtc(nameof(Min), (DateTime)value);
                return true;
            });

        public static readonly DependencyProperty MaxProperty =
            DependencyProperty.Register("Max", typeof(DateTime),
            typeof(DateTimePeriodViewModel),
            new UIPropertyMetadata(DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc),
                MaxPropertyChanged,
                CoerceMaxValue),
            delegate(object value)
            {
                InParameter.VerifyDateIsUtc(nameof(Max), (DateTime)value);
                return true;
            });



        public static readonly DependencyProperty StartProperty =
            DependencyProperty.Register("Start",
            typeof(DateTime),
            typeof(DateTimePeriodViewModel),
            new UIPropertyMetadata(DateTime.UtcNow, StartPropertyChanged, CoerceStartValue),
            delegate(object value)
            {
                value = TimeZoneHelper.ConvertToUtc((DateTime)value, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
                //InParameter.VerifyDateIsUtc("Start", (DateTime)value);
                return true;
            });



        public static readonly DependencyProperty EndProperty =
            DependencyProperty.Register("End",
            typeof(DateTime),
            typeof(DateTimePeriodViewModel),
            new UIPropertyMetadata(DateTime.UtcNow, EndPropertyChanged, CoerceEndValue),
            delegate(object value)
            {
                value = TimeZoneHelper.ConvertToUtc((DateTime)value, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
                InParameter.VerifyDateIsUtc(nameof(End), (DateTime)value);
                return true;
            });


        #endregion //Registration

        #region Coerce & Validation

        #region Coerce
        private static object CoerceMaxValue(DependencyObject d, object baseValue)
        {
            return baseValue;
        }

        private static object CoerceMinValue(DependencyObject d, object baseValue)
        {
            return baseValue;
        }

        private static object CoerceStartValue(DependencyObject d, object baseValue)
        {
            DateTimePeriodViewModel model = (DateTimePeriodViewModel)d;
            DateTime newValue = (DateTime)baseValue;
            if (model.AutoUpdate)
            {

                if (newValue > model.Max) return model.Max;
                if (newValue < model.Min) return model.Min;

            }
            return newValue;
        }

        private static object CoerceEndValue(DependencyObject d, object baseValue)
        {
            DateTimePeriodViewModel model = (DateTimePeriodViewModel)d;
            DateTime newValue = (DateTime)baseValue;
            if (model.AutoUpdate)
            {
                if (newValue > model.Max) return model.Max;
                if (newValue < model.Min) return model.Min;
                if (newValue.Add(model.Interval) < model.Start) newValue = newValue.Add(model.Interval);
            }

            return newValue;
        }
        #endregion //Coerce

        #region changed
        private static void MaxPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            DateTimePeriodViewModel model = (DateTimePeriodViewModel)d;
            if (e.NewValue != e.OldValue)
            {
                model.MinMax = new MinMax<DateTime>(model.Min, model.Max);
                d.CoerceValue(EndProperty);
            }
        }

        private static void MinPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DateTimePeriodViewModel model = (DateTimePeriodViewModel)d;
            if (e.NewValue != e.OldValue)
            {
                model.MinMax = new MinMax<DateTime>(model.Min, model.Max);
                d.CoerceValue(StartProperty);
            }
        }

        private static void StartPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DateTime newValue = (DateTime)e.NewValue;
            DateTimePeriodViewModel model = (DateTimePeriodViewModel)d;
            if (model.AutoUpdate)
            {
                var duration = model.End - model.Start;
                if (duration.Minutes <= 0) duration = model.Interval;
                if (model.End < newValue.Add(model.Interval)) model.End = model.Start.Add(duration);
                if (e.NewValue != e.OldValue) model.DateTimePeriod = new Domain.InterfaceLegacy.Domain.DateTimePeriod(model.Start, model.End);
            }
            UpdateIsValid(model);
        }
        private static void EndPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DateTime newValue = (DateTime)e.NewValue;
            DateTimePeriodViewModel model = (DateTimePeriodViewModel)d;
            if (model.AutoUpdate)
            {
                var duration = model.End - model.Start;
                if (duration.Minutes <= 0) duration = model.Interval;
                if (model.Start.Add(model.Interval) > newValue) model.Start = model.End.Subtract(duration);
                if (e.NewValue != e.OldValue) model.DateTimePeriod = new Domain.InterfaceLegacy.Domain.DateTimePeriod(model.Start, model.End);
            }
            UpdateIsValid(model);
        }

        private static void UpdateIsValid(DateTimePeriodViewModel model)
        {
            model.IsValid = model.Start < model.End && model.Start >= model.Min && model.Start <= model.Max;
        }

        private static void IntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeSpan interval = (TimeSpan)e.NewValue;
            DateTimePeriodViewModel model = (DateTimePeriodViewModel)d;
            if (model.AutoUpdate)
            {
                if (model.Start.Add(interval) > model.End) model.End = model.End.Add(interval);
            }
            //recalculate the SelectableTimes:
            model.CalculateSelectableTimeSpans();

        }
        #endregion //changed
        #endregion Coerce & Validation
        #endregion //DependencyProperties

        #region ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimePeriodViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-09-09
        /// </remarks>
        public DateTimePeriodViewModel()
        {
            _dateTimePeriod = new Domain.InterfaceLegacy.Domain.DateTimePeriod(Start, End);
            StartDateTimeViewModel = new DateTimeViewModel(Start);
            EndDateTimeViewModel = new DateTimeViewModel(End);
            _minMax = new MinMax<DateTime>(Min, Max);
            CalculateSelectableTimeSpans();
            SetupForCommandModels();
            SetupForSelectables();
        }

        private void SetupForSelectables()
        {
            SelectableEndTimes = new ListCollectionView(_selectableTimeSpans);
            SelectableStartTimes = new ListCollectionView(_selectableTimeSpans);
            SelectableStartTimes.CurrentChanged += SelectableStartTimes_CurrentChanged;
            SelectableEndTimes.CurrentChanged += SelectableEndTimes_CurrentChanged;
        }

        void SelectableStartTimes_CurrentChanged(object sender, EventArgs e)
        {
            if (SelectableStartTimes.CurrentItem != null)
            {
                //TimeSpan timeOfDay;
				//if (TimeHelper.TryParse(SelectableStartTimes.CurrentItem.ToString(), out timeOfDay))
				//    //StartTimeAsLocalTimeSpan = timeOfDay;
            }
        }

        void SelectableEndTimes_CurrentChanged(object sender, EventArgs e)
        {
            if (SelectableEndTimes.CurrentItem != null)
            {
                //TimeSpan timeOfDay;
				//if (TimeHelper.TryParse(SelectableEndTimes.CurrentItem.ToString(), out timeOfDay)) 
				//   // EndTimeAsLocalTimeSpan = timeOfDay;
            }
        }

        private void CalculateSelectableTimeSpans()
        {
            TimeSpan selectableTimeSpan = Interval < MinimumInterval ? MinimumInterval : Interval;
            if(selectableTimeSpan > MaximumInterval )selectableTimeSpan = MaximumInterval;
            _selectableTimeSpans.Clear();
            DateTime maxTime = DateTime.MinValue.Add(new TimeSpan(23, 59, 59).Add(new TimeSpan(1)));
            for (DateTime timeOfDay = DateTime.MinValue.Add(TimeSpan.Zero);
                timeOfDay < maxTime;
                timeOfDay = timeOfDay.AddMinutes(selectableTimeSpan.TotalMinutes))
            {
                _selectableTimeSpans.Add(timeOfDay.ToShortTimeString());
            }
        }

        private void SetupForCommandModels()
        {
            MoveStartTimeEarlierCommand = CommandModelFactory.CreateCommandModel(() => { Start = Start.Subtract(Interval).ToInterval(Interval); },
                                                                            CommonRoutedCommands.
                                                                                MoveStartOneIntervalEarlier);

            MoveStartTimeLaterCommand = CommandModelFactory.CreateCommandModel(() => { Start = Start.Add(Interval).ToInterval(Interval); },
                                                                    CommonRoutedCommands.
                                                                        MoveStartOneIntervalLater);

            MoveEndTimeEarlierCommand = CommandModelFactory.CreateCommandModel(() => { End = End.Subtract(Interval).ToInterval(Interval); },
                                                               CommonRoutedCommands.
                                                                   MoveEndOneIntervalEarlier);

            MoveEndTimeLaterCommand = CommandModelFactory.CreateCommandModel(() => { End = End.Add(Interval).ToInterval(Interval); },
                                                                    CommonRoutedCommands.
                                                                        MoveEndOneIntervalLater);

            MovePeriodEarlierCommand = CommandModelFactory.CreateCommandModel(() =>
            {
                Start = Start.Subtract(Interval).ToInterval(Interval);
                End = End.Subtract(Interval).ToInterval(Interval);
            }, CommonRoutedCommands.MovePeriodOneIntervalEarlier);

            MovePeriodLaterCommand = CommandModelFactory.CreateCommandModel(() =>
                                                                                {
                                                                                    Start = Start.Add(Interval).ToInterval(Interval);
                                                                                    End = End.Add(Interval).ToInterval(Interval);

                                                                                }, CommonRoutedCommands.MovePeriodOneIntervalLater);

            ToggleAutoUpdatePeriod = CommandModelFactory.CreateCommandModel(() => { AutoUpdate = !AutoUpdate; }, CommonRoutedCommands.ToggleAutoUpdate);


        }

        #endregion //ctor

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string arg)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(arg));
			}
        }

        #endregion //PropertyChanged

        #region Bindables for windows forms
        //henrik: Needs to be properties for everything we intend to bind directly to in windows forms
        //office2007TimePicker uses local TimeSpan
        //dateTimePickerAdv uses local datetimes
        public DateTime StartDateTimeAsLocal
        {
            get { return TimeZoneHelper.ConvertFromUtc(Start, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone); }
            set { SetValue(StartProperty, TimeZoneHelper.ConvertToUtc(value, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone)); }
        }

        public DateTime EndDateTimeAsLocal
        {
            get { return TimeZoneHelper.ConvertFromUtc(End, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone); }
            set { SetValue(EndProperty, TimeZoneHelper.ConvertToUtc(value, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone)); }
        }

        public TimeSpan StartTimeAsLocalTimeSpan
        {
            set
            {
                StartDateTimeAsLocal = StartDateTimeAsLocal.Date.Add(value);
            }
            get { return StartDateTimeAsLocal.TimeOfDay; }
        }

        public TimeSpan EndTimeAsLocalTimeSpan
        {
            set
            {
                EndDateTimeAsLocal = EndDateTimeAsLocal.Date.Add(value);

            }
            get { return EndDateTimeAsLocal.TimeOfDay; }
        }

        public bool SameDateUtc
        {
            get { return _sameDateUtc; }
            set
            {
                if (_sameDateUtc != value)
                {
                    _sameDateUtc = value;
                    NotifyPropertyChanged(nameof(SameDateUtc));
                }
            }
        }

        public bool SameDateLocal
        {
            get { return _sameDateLocal; }
            set
            {
                if (_sameDateLocal != value)
                {
                    _sameDateLocal = value;
                    NotifyPropertyChanged(nameof(SameDateLocal));
                }
            }
        }

        public IList<TimeSpan> SelectableTimes
        {
            get
            {
                IList<TimeSpan> ret = new List<TimeSpan>();
                TimeSpan selectableTimeSpan = Interval < MinimumInterval ? MinimumInterval : Interval;
                if (selectableTimeSpan > MaximumInterval) selectableTimeSpan = MaximumInterval;

                for (TimeSpan t = TimeSpan.Zero; t < TimeSpan.FromHours(24); t = t.Add(selectableTimeSpan))
                {
                    ret.Add(t);
                }
                return ret;
            }

        }

        public bool IsValid
        {
            get { return _isValid; }
            private set
            {
                if (_isValid != value)
                {
                    _isValid = value;
                    NotifyPropertyChanged(nameof(IsValid));
                }
            }
        }

        #endregion //Bindables for windows forms

        public string this[string columnName]
        {
            get
            {
                return IsValid ? null : UserTexts.Resources.StartDateMustBeSmallerThanEndDate;
            }
        }

        public string Error
        {
            get
            {
                if(IsValid) return null;
                return InvalidMessage;
            }
        }

        public string InvalidMessage
        {
            get { return UserTexts.Resources.StartDateMustBeSmallerThanEndDate; }
        }

       
    }
}

