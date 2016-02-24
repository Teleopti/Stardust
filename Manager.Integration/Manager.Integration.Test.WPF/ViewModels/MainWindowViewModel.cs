using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using Manager.Integration.Test.WPF.Annotations;
using Manager.Integration.Test.WPF.Commands;

namespace Manager.Integration.Test.WPF.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private const string WorkerNodeHeaderConstant = "Worker Node";
        private const string LoggingHeaderConstant = "Logging";
        private const string JobHistoryHeaderConstant = "Job History";
        private const string JobHistoryDetailHeaderConstant = "Job History Detail";
        private const string JobDefinitionHeaderConstant = "Job Definition";

        public string WorkerNodeHeader
        {
            get { return _workerNodeHeader; }
            set
            {
                _workerNodeHeader = value;

                OnPropertyChanged();
            }
        }

        private string _status;

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;

                OnPropertyChanged();
            }
        }


        public MainWindowViewModel()
        {
            GetData();

            ClearDatabaseCommand = new ClearDatabaseCommand(this);
            ToggleRefreshCommand = new ToggleRefreshCommand(this);

            RefreshTimer = new Timer(5000);

            RefreshTimer.Elapsed += RefreshTimerOnElapsed;

            RefreshTimer.Start();

            RefreshEnabled = true;
        }

        private void RefreshTimerOnElapsed(object sender,
                                           ElapsedEventArgs elapsedEventArgs)
        {
            RefreshTimer.Stop();

            try
            {
                GetData();
            }

            finally
            {
                RefreshTimer.Start();
            }
        }

        public Timer RefreshTimer { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                                new PropertyChangedEventArgs(propertyName));
            }
        }

        private List<JobHistory> _jobHistoryData;
        private List<JobHistoryDetail> _jobHistoryDetailData;
        private List<JobDefinition> _jobDefinitionData;
        private List<WorkerNode> _workerNodesData;
        private string _workerNodeHeader;
        private string _loggingHeader;
        private string _jobHistoryHeader;
        private string _jobHistoryDetailHeader;
        private string _jobDefinitionDataHeader;
        private ClearDatabaseCommand _clearDatabaseCommand;
        private List<Logging> _loggingData;
        private ToggleRefreshCommand _toggleRefreshCommand;
        private string _toggleRefreshStatus;
        private bool _refreshEnabled;
        private int _refreshProgressValue;

        public ClearDatabaseCommand ClearDatabaseCommand
        {
            get { return _clearDatabaseCommand; }
            set
            {
                _clearDatabaseCommand = value;

                OnPropertyChanged();
            }
        }

        public List<Logging> LoggingData
        {
            get { return _loggingData; }
            set
            {
                _loggingData = value;

                OnPropertyChanged();
            }
        }

        private void GetData()
        {
            RefreshProgressValue = 0;

            Status = "Refresh started...";

            using (ManagerDbEntities managerDbEntities = new ManagerDbEntities())
            {
                LoggingData =
                    managerDbEntities.Loggings.OrderByDescending(logging => logging.Id)
                        .ToList();

                ++RefreshProgressValue;

                LoggingHeader = LoggingHeaderConstant + " ( " + LoggingData.Count + " )";

                JobHistoryData =
                    managerDbEntities.JobHistories.OrderByDescending(history => history.Created)
                        .ToList();

                ++RefreshProgressValue;

                JobHistoryHeader = JobHistoryHeaderConstant + " ( " + JobHistoryData.Count + " )";

                JobHistoryDetailData =
                    managerDbEntities.JobHistoryDetails.OrderByDescending(history => history.Created)
                        .ToList();


                ++RefreshProgressValue;

                JobHistoryDetailHeader = JobHistoryDetailHeaderConstant + " ( " + JobHistoryDetailData.Count + " )";

                JobDefinitionData =
                    managerDbEntities.JobDefinitions
                        .ToList();


                ++RefreshProgressValue;

                JobDefinitionDataHeader = JobDefinitionHeaderConstant + " ( " + JobDefinitionData.Count + " )";

                WorkerNodesData =
                    managerDbEntities.WorkerNodes
                        .ToList();

                ++RefreshProgressValue;

                WorkerNodeHeader = WorkerNodeHeaderConstant + " ( " + WorkerNodesData.Count + " )";
            }

            Status = "Refresh finished.";
        }

        public string JobDefinitionDataHeader
        {
            get { return _jobDefinitionDataHeader; }
            set
            {
                _jobDefinitionDataHeader = value;

                OnPropertyChanged();
            }
        }

        public string JobHistoryDetailHeader
        {
            get { return _jobHistoryDetailHeader; }
            set
            {
                _jobHistoryDetailHeader = value;

                OnPropertyChanged();
            }
        }

        public string JobHistoryHeader
        {
            get { return _jobHistoryHeader; }
            set
            {
                _jobHistoryHeader = value;

                OnPropertyChanged();
            }
        }

        public string LoggingHeader
        {
            get { return _loggingHeader; }
            set
            {
                _loggingHeader = value;

                OnPropertyChanged();
            }
        }

        public List<WorkerNode> WorkerNodesData
        {
            get { return _workerNodesData; }
            set
            {
                _workerNodesData = value;

                OnPropertyChanged();
            }
        }

        public List<JobDefinition> JobDefinitionData
        {
            get { return _jobDefinitionData; }
            set
            {
                _jobDefinitionData = value;

                OnPropertyChanged();
            }
        }

        public List<JobHistoryDetail> JobHistoryDetailData
        {
            get { return _jobHistoryDetailData; }

            set
            {
                _jobHistoryDetailData = value;

                OnPropertyChanged();
            }
        }

        public List<JobHistory> JobHistoryData
        {
            get { return _jobHistoryData; }

            set
            {
                _jobHistoryData = value;

                OnPropertyChanged();
            }
        }

        public bool DatabaseContainsInformation()
        {
            return true;
        }

        public void DatabaseClearAllInformation()
        {
            using (ManagerDbEntities managerDbEntities = new ManagerDbEntities())
            {
                managerDbEntities.Database.ExecuteSqlCommand("TRUNCATE TABLE Stardust.Logging");
            }

            GetData();
        }

        public ToggleRefreshCommand ToggleRefreshCommand
        {
            get { return _toggleRefreshCommand; }
            set
            {
                _toggleRefreshCommand = value;

                OnPropertyChanged();
            }
        }

        public bool RefreshEnabled
        {
            get { return _refreshEnabled; }
            set
            {
                _refreshEnabled = value;

                if (_refreshEnabled)
                {
                    ToggleRefreshStatus = "Refresh Enabled";

                    RefreshTimer.Start();
                }
                else
                {
                    ToggleRefreshStatus = "Refresh Disabled";

                    RefreshProgressValue = 0;

                    RefreshTimer.Stop();
                }

                OnPropertyChanged();
            }
        }

        public string ToggleRefreshStatus
        {
            get { return _toggleRefreshStatus; }

            set
            {
                _toggleRefreshStatus = value;

                OnPropertyChanged();
            }
        }

        public int RefreshProgressValue
        {
            get { return _refreshProgressValue; }
            set
            {
                _refreshProgressValue = value;

                OnPropertyChanged();
            }
        }

        public void ToggleRefresh()
        {
            RefreshEnabled = !RefreshEnabled;
        }
    }
}