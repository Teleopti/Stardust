using System.Collections.ObjectModel;
using System.Windows;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.ScheduleViewer.Models
{
    public class ScheduleRangeViewModel : DependencyObject
    {
        #region fields
        private ObservableCollection<ScheduleViewerScheduleDayViewModel> _parts = new ObservableCollection<ScheduleViewerScheduleDayViewModel>();
        private IScheduleRange _range;

        public static readonly DependencyProperty ShowScheduleProperty =
          DependencyProperty.Register("ShowSchedule", typeof(Visibility), typeof(ScheduleRangeViewModel), new UIPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty ShowOverviewProperty =
            DependencyProperty.Register("ShowOverview", typeof(Visibility), typeof(ScheduleRangeViewModel), new UIPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty ShowDetailsProperty =
           DependencyProperty.Register("ShowDetails", typeof(Visibility), typeof(ScheduleRangeViewModel), new UIPropertyMetadata(Visibility.Collapsed));

        #endregion

        #region properties
        public ObservableCollection<ScheduleViewerScheduleDayViewModel> Parts
        {
            get { return _parts; }
        }

        public string Person
        {
            get { return _range.Person.Name.ToString(); }
        }
        public Visibility ShowDetails
        {
            get { return (Visibility)GetValue(ShowDetailsProperty); }
            set { SetValue(ShowDetailsProperty, value); }
        }


        public Visibility ShowSchedule
        {
            get { return (Visibility)GetValue(ShowScheduleProperty); }
            set { SetValue(ShowScheduleProperty, value); }
        }


        public Visibility ShowOverview
        {
            get { return (Visibility)GetValue(ShowOverviewProperty); }
            set { SetValue(ShowOverviewProperty, value); }
        }

        #endregion

        public ScheduleRangeViewModel(IScheduleRange range, DateTimePeriod period)
        {
            _range = range;
            foreach (DateOnly date in period.ToDateOnlyPeriod(range.Person.PermissionInformation.DefaultTimeZone()).DayCollection())
            {
                Parts.Add(new ScheduleViewerScheduleDayViewModel(range.ScheduledDay(date), null));
            }
        }
    }
}
