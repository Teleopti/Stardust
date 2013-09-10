using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WpfControls.Controls.Time.Timeline
{
    /// <summary>
    /// Interaction logic for TimeControl.xaml
    /// Holds Timeline
    /// </summary>
    public partial class TimeControl : UserControl,INotifyPropertyChanged
    {
        #region fields

        private ObservableCollection<ILayer> _layers = new ObservableCollection<ILayer>();
        private DateTimePeriod _selecteDateTimePeriod = new DateTimePeriod();

        #endregion //fields

        #region properties
        public void SetLayer(ILayer layer)
        {
            _layers.Clear();
            _layers.Add(layer);
            NotifyPropertyChanged("Layers");
        }


        /// <summary>
        /// Gets or sets the layers.
        /// Added layer will be shown as gray in the timeline
        /// </summary>
        /// <value>The layers.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-07-07
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ObservableCollection<ILayer> Layers
        {
            get { return _layers; }
            set
            {
                _layers = value;
                NotifyPropertyChanged("Layers");
            }
        }
        #endregion //properties
        
        #region dependencyproperties
        /// <summary>
        /// Gets or sets the interval.
        /// This is a dependencyproperty
        /// </summary>
        /// <value>The interval.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-06-26
        /// </remarks>
        public TimeSpan Interval
        {
            get { return (TimeSpan)GetValue(IntervalProperty); }
            set
            {
                SetValue(IntervalProperty, value);
                
            }
        }

     


        /// <summary>
        /// Gets or sets the date time period.
        /// Thisis a dependencyproperty
        /// </summary>
        /// <value>The date time period.</value>
        public DateTimePeriod DateTimePeriod
        {
            get { return (DateTimePeriod)GetValue(DateTimePeriodProperty); }
            set 
            { 
                SetValue(DateTimePeriodProperty, value);
               
            }
        }

        private static void DateTimePeriodChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeControl timeControl = (TimeControl) d;
            timeControl.initTimeLinePanel();
            timeControl.setTickMarks();
        }


        /// <summary>
        /// Gets or sets a value indicating whether to Show/Hide tickmarks.
        /// This is a dependencyproperty
        /// </summary>
        /// <value><c>true</c> if [show tick mark]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-06-26
        /// </remarks>
        public bool ShowTickMark
        {
            get { return (bool) GetValue(ShowTickMarkProperty);}
            set {SetValue(ShowTickMarkProperty, value);}
        }


        public DateTimePeriod SelectedPeriod
        {
            get { return _selecteDateTimePeriod; }
            set
            {
                if (value!=_selecteDateTimePeriod)
                {
                    _selecteDateTimePeriod = value;
                    NotifyPropertyChanged("SelectedPeriod");
                }
            }
        }

        /// <summary>
        /// Gets or sets up time.(DependencyProperty)
        /// Use for selecting times
        /// </summary>
        /// <value>Up time.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-09-04
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "UpTime")]
        public DateTime MouseUpTime
        {
            get { return (DateTime)GetValue(MouseUpTimeProperty); }
            set
            {
                SetValue(MouseUpTimeProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets down time.(DependencyProperty)
        /// Use for selecting times
        /// </summary>
        /// <value>Down time.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-09-04
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "DownTime")]
        public DateTime MouseDownTime
        {
            get { return (DateTime)GetValue(MouseDownTimeProperty); }
            set
            {
                SetValue(MouseDownTimeProperty, value);
            }
        }

        /// <summary>
        /// DependencyProperty for showing/hiding tickmarks
        /// </summary>
        public static readonly DependencyProperty ShowTickMarkProperty =
            DependencyProperty.Register("ShowTickMark",
                                        typeof (bool),
                                        typeof (TimeControl),
                                        new FrameworkPropertyMetadata(true));

        /// <summary>
        /// DependencyProperty for Tickmark-intervals
        /// </summary>
        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register("Interval",
                typeof(TimeSpan),
                typeof(TimeControl),
                new FrameworkPropertyMetadata(TimeSpan.FromMinutes(15)));


        /// <summary>
        /// DependencyProperty for the DateTimePeriod to show
        /// </summary>
        public static readonly DependencyProperty DateTimePeriodProperty =
            DependencyProperty.Register("DateTimePeriod", 
            typeof(DateTimePeriod), 
            typeof(TimeControl), 
            new FrameworkPropertyMetadata(new DateTimePeriod(DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc),
                DateTime.SpecifyKind(DateTime.MinValue.AddMinutes(1), DateTimeKind.Utc)), FrameworkPropertyMetadataOptions.AffectsParentArrange,new PropertyChangedCallback(DateTimePeriodChanged)));

       

        /// <summary>
        /// DependencyProperty for UpTime
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "UpTime")]
        public static readonly DependencyProperty MouseUpTimeProperty =
            DependencyProperty.Register("MouseUpTime",
                                        typeof (DateTime),
                                        typeof (TimeControl),
                                        new FrameworkPropertyMetadata(DateTime.SpecifyKind(DateTime.MinValue,
                                                                                           DateTimeKind.Utc), 
                                                                                           new PropertyChangedCallback(UpTimeChanged)));



        /// <summary>
        /// DependencyProperty for DownTime
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "DownTime")]
        public static readonly DependencyProperty MouseDownTimeProperty =
          DependencyProperty.Register("MouseDownTime",
                                      typeof(DateTime),
                                      typeof(TimeControl),
                                      new FrameworkPropertyMetadata(DateTime.SpecifyKind(DateTime.MinValue,
                                                                                         DateTimeKind.Utc),new PropertyChangedCallback(DownTimeChanged)));

       
        private static void DownTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeControl control = (TimeControl)d;
            control.SelectedPeriod = CreateDateTimePeriod((DateTime)e.NewValue, control.MouseUpTime);
        }

        private static void UpTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeControl control = (TimeControl) d;
            control.SelectedPeriod = CreateDateTimePeriod((DateTime)e.NewValue, control.MouseDownTime);
        }


        private static DateTimePeriod CreateDateTimePeriod(DateTime first,DateTime second)
        {
            InParameter.VerifyDateIsUtc("first", first);
            InParameter.VerifyDateIsUtc("second", second);
            if (second >= first)
            {
                return new DateTimePeriod(first,second);
            }
            return new DateTimePeriod(second,first);
        }

        #endregion //dependencyproperties


        public TimeControl()
        {
            InitializeComponent();
        }

        private void initTimeLinePanel()
        {

          
            TimeLinePanel.Children.Clear();
            DateTimePeriodPanel.SetDateTimePeriod(TimeLinePanel, DateTimePeriod);
            foreach (DateTimePeriod d in EvenHourPeriodsInCurrentTimeZone())
            {
              
                ContentPresenter cp = new ContentPresenter();
                DateTimePeriodPanel.SetDateTimePeriod(cp, d);
                cp.Content = d;
                TimeLinePanel.Children.Add(cp);
            }
        }

        private void setTickMarks()
        {
            //Minortickmarks will not be set on hour and will start from every whole hour

            TickMarkPanel.Children.Clear();
            foreach (DateTimePeriod d in EvenHourPeriodsInCurrentTimeZone())
            {
                DateTime tick = d.StartDateTime.Add(Interval);

                while (d.EndDateTime > tick)
                {
                    ContentPresenter cp = new ContentPresenter();
                    DateTimePeriodPanel.SetDateTimePeriod(cp, new DateTimePeriod(tick, tick.AddMinutes(1)));
                    cp.Content = tick;
                    TickMarkPanel.Children.Add(cp);
                    tick = tick.Add(Interval);
                }
            }
        }

        /// <summary>
        /// Exposes the tickmarkpanels position to time
        /// </summary>
        /// <param name="positionX">The x.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-09-04
        /// </remarks>
        private DateTime GetDateTimeFromPosition(double positionX)
        {
            return TimeLinePanel.GetUtcDateTimeFromPosition(positionX);
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-07-07
        /// </remarks>
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string property)
        {
        	var handler = PropertyChanged;
            if (handler!=null)
        	{
        		handler.Invoke(this,new PropertyChangedEventArgs(property));
        	}

        }

        #endregion


        private IList<DateTimePeriod> EvenHourPeriodsInCurrentTimeZone()
        {
            //We must check from the Gui if the timezone has different minutes:
            //Henrik 2009-04-01 This needs some refactoring and thinking
            return DateTimePeriod.AffectedHourCollection();
        }

        private void TimeLinePanel_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MouseUpTime = TimeZoneHelper.ConvertToUtc(GetDateTimeFromPosition(e.GetPosition(TimeLinePanel).X));
        }
        private void TimeLinePanel_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MouseDownTime = TimeZoneHelper.ConvertToUtc(GetDateTimeFromPosition(e.GetPosition(TimeLinePanel).X));
        }
    }
}
