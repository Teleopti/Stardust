using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Panels;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Time
{
    public class TimeZoomViewModel : DependencyObject, ITimeZoomViewModel
    {
        #region Properties
        public ObservableCollection<ITimeZoomItemViewModel> ZoomLevels { get; private set; }
        public CommandModel ZoomInCommand { get; private set; }
        public CommandModel ZoomOutCommand { get; private set; }
        public CommandModel ScrollToDateTimeCommand { get; private set; }
        public CommandModel ScrollToNowCommand { get; private set; }

        public DateTimePeriod Period
        {
            get { return (DateTimePeriod) GetValue(PeriodProperty); }
            set { SetValue(PeriodProperty, value); }
        }

        public double PanelWidth
        {
            get { return (double) GetValue(PanelWidthProperty); }
            set { SetValue(PanelWidthProperty, value); }
        }

        /// <summary>
        /// Gets or sets the scroll position.
        /// This is double between 0-1 that can be used for scrolling where 1 is the EndDateTime of the Period and 0 is the StartDateTime
        /// </summary>
        /// <value>The scroll position.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-07-07
        /// </remarks>
        public double ScrollPosition
        {
            get { return (double) GetValue(ScrollPositionProperty); }
            set { SetValue(ScrollPositionProperty, value); }
        }

        public static readonly DependencyProperty ScrollPositionProperty =
            DependencyProperty.Register("ScrollPosition", typeof(double), typeof(TimeZoomViewModel), new UIPropertyMetadata(0d, UpdateBindingsForPanelScrollPosition));

        public DateTime ScrollDateTime
        {
            get { return (DateTime) GetValue(ScrollDateTimeProperty); }
            set { SetValue(ScrollDateTimeProperty, value); }
        }

        public static readonly DependencyProperty ScrollDateTimeProperty =
            DependencyProperty.Register("ScrollDateTime", typeof (DateTime), typeof (TimeZoomViewModel), new UIPropertyMetadata(DateTime.UtcNow));

        public double PanelScrollPosition
        {
            get { return ScrollPosition * PanelWidth; }

        }

        #endregion

        public TimeZoomViewModel(DateTimePeriod period)
        {

            ZoomLevels = new ObservableCollection<ITimeZoomItemViewModel>(){new TimeZoomItemViewModel(0.5d),new TimeZoomItemViewModel(1d),new TimeZoomItemViewModel(2d)};
            ZoomLevel = ZoomLevels[1];
            Period = period;
            SetUpCommandModels();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxZoomOut"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxZoomIn")]
        private void SetUpCommandModels()
        {
            ZoomInCommand = CommandModelFactory.CreateCommandModel(ZoomInExecuted, ZoomInCanExecute, "xxZoomIn");
            ZoomOutCommand = CommandModelFactory.CreateCommandModel(ZoomOutExecuted,ZoomOutCanexecute, "xxZoomOut");
       
            ScrollToDateTimeCommand = CommandModelFactory.CreateCommandModel(ScrollToDateTimeExecuted,ScrollToDateTimeCanExecute, String.Empty);
            ScrollToNowCommand = CommandModelFactory.CreateCommandModel(ScrollToNowCommandExecuted,ScrollToNowCommandCanExecute,UserTexts.Resources.NowColon);
        }

        private bool ScrollToNowCommandCanExecute()
        {
            return Period.Contains(DateTime.UtcNow);
        }

        private void ScrollToNowCommandExecuted()
        {
            ScrollDateTime = DateTime.UtcNow;
            ScrollToDateTimeExecuted();
        }

        private bool ScrollToDateTimeCanExecute()
        {
            return Period.ContainsPart(ScrollDateTime);
        }

        private void ScrollToDateTimeExecuted()
        {
            ScrollPosition = new LengthToTimeCalculator(Period, 1d).PositionFromDateTime(ScrollDateTime);
        }

        #region Commands
        private bool ZoomOutCanexecute()
        {
            return ZoomLevel != ZoomLevels.Last(); 
        }

        private void ZoomOutExecuted()
        {
            ZoomLevel = ZoomLevels[ZoomLevels.IndexOf(ZoomLevel) + 1];
            
            
        }

        private bool ZoomInCanExecute()
        {
            return ZoomLevel != ZoomLevels.First();
        }

        private void ZoomInExecuted()
        {
            ZoomLevel = ZoomLevels[ZoomLevels.IndexOf(ZoomLevel) - 1];
        }
        #endregion

        #region dependenyproperties
        public static readonly DependencyProperty PeriodProperty =
            DependencyProperty.Register("Period", typeof(DateTimePeriod), typeof(TimeZoomViewModel), new UIPropertyMetadata(new DateTimePeriod(),PeriodChanged));

     
        public static readonly DependencyProperty PanelWidthProperty =
          DependencyProperty.Register("PanelWidth", typeof(double), typeof(TimeZoomViewModel), new UIPropertyMetadata(1d,UpdateBindingsForPanelScrollPosition));

        private static void UpdateBindingsForPanelScrollPosition(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TimeZoomViewModel)d).NotifyPropertyChanged(nameof(PanelScrollPosition)); 
        }

        #region coerce & changed
        private static void PeriodChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeZoomViewModel model = (TimeZoomViewModel) d;
            model.RecalculatePanelWidth();
        }

        private void RecalculatePanelWidth()
        {
            PanelWidth = Period.ElapsedTime().TotalMinutes*MinuteWidth;
        }

        #endregion
        #endregion

        #region ITimeZoomViewModel Members


        public double MinuteWidth
        {
            get { return ZoomLevel.MinuteWidth; }
        }

        public ITimeZoomItemViewModel ZoomLevel
        {
            get { return CollectionViewSource.GetDefaultView(ZoomLevels).CurrentItem as ITimeZoomItemViewModel; }
            private set
            {
                (CollectionViewSource.GetDefaultView(ZoomLevels)).MoveCurrentTo(value);
                RecalculatePanelWidth();
            }
        }

        #endregion

        #region PropertyChanged
        private void NotifyPropertyChanged(string property)
        {
        	var handler = PropertyChanged;
            if (handler!=null)
            {
            	handler(this,new PropertyChangedEventArgs(property));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
