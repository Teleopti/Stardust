using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Models;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.ScheduleViewer.Models
{
    public class ScheduleViewerViewModel : DataModel
    {
        public DateTimePeriod SelectedPeriod { get; set; }
        
        public bool ShowSchedules
        {
            get { return _showSchedules;} 
            private set
            {
                _showSchedules = value;
                SendPropertyChanged(nameof(ShowSchedules));
                foreach (ScheduleRangeViewModel v in Ranges)
                {
                    v.ShowSchedule = ShowSchedules ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }
        public bool ShowDays
        {
            get { return _showDays; }
            private set
            {
                _showDays = value;
                SendPropertyChanged(nameof(ShowDays));
                foreach(ScheduleRangeViewModel v in Ranges)
                {
                    v.ShowOverview = ShowDays ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }
        public bool ShowDetails
        {
            get
            {
                return _showDetails;
            }
            private set
            {
                _showDetails = value;
                SendPropertyChanged(nameof(ShowDetails));
                foreach (ScheduleRangeViewModel v in Ranges)
                {
                    v.ShowDetails = ShowDetails ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        public ObservableCollection<ScheduleRangeViewModel> Ranges { get; private set; }

        public CommandModel ToggleShowSchedules
        {
            get
            {
                return _toggleShowSchedulesCommandModel;
            }
        }
        public CommandModel ToggleShowDays
        {
            get
            {
                return _toggleShowDaysCommandModel;
            }
        }
        public CommandModel ToggleShowDetails
        {
            get
            {
                return _toggleShowDetailsCommandModel;
            }
        }

        public ScheduleViewerViewModel(IScheduleDictionary dictionary)
        {
            SelectedPeriod = dictionary.Period.VisiblePeriod;
            Ranges = new ObservableCollection<ScheduleRangeViewModel>();
             _toggleShowSchedulesCommandModel= new ToggleShowSchedulesCommandModel(this);
             _toggleShowDaysCommandModel = new ToggleShowDaysCommandModel(this);
             _toggleShowDetailsCommandModel = new ToggleShowDetailsCommandModel(this);


            foreach (IScheduleRange range in dictionary.Values)
            {
                Ranges.Add(new ScheduleRangeViewModel(range, SelectedPeriod));
            }
        }

        #region commanding
        #region fields
        private ToggleShowSchedulesCommandModel _toggleShowSchedulesCommandModel;
        private ToggleShowDaysCommandModel _toggleShowDaysCommandModel;
        private ToggleShowDetailsCommandModel _toggleShowDetailsCommandModel;
        private bool _showSchedules;
        private bool _showDays;
        private bool _showDetails;
        #endregion

        private class ToggleShowSchedulesCommandModel : CommandModel
        {
            private ScheduleViewerViewModel _model;
           

            public ToggleShowSchedulesCommandModel(ScheduleViewerViewModel model)
            {
                _model = model;
            }
            public override string Text
            {
                get { return "xxShowSchedules"; }
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                _model.ShowSchedules = !_model.ShowSchedules;
            }
        }

        private class ToggleShowDaysCommandModel : CommandModel
        {
            private ScheduleViewerViewModel _model;
            

            public ToggleShowDaysCommandModel(ScheduleViewerViewModel model)
            {
                _model = model;
            }
            public override string Text
            {
                get { return "xxShowDays"; }
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                _model.ShowDays = !_model.ShowDays;
            }
        }

        private class ToggleShowDetailsCommandModel : CommandModel
        {
            private ScheduleViewerViewModel _model;
            

            public ToggleShowDetailsCommandModel(ScheduleViewerViewModel model)
            {
                _model = model;
            }
            public override string Text
            {
                get { return "xxShowDetails"; }
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                _model.ShowDetails = !_model.ShowDetails;
            }
        }
        #endregion;
    }
}
