using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            ClearDatabaseCommand = new ClearDatabaseCommand(this);

            RefreshTimer = new Timer(5000);

            RefreshTimer.Elapsed += RefreshTimerOnElapsed;

            RefreshTimer.Start();
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
            if (this.PropertyChanged != null)
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
            Status = "Refresh started...";

            using (ManagerDbEntities managerDbEntities = new ManagerDbEntities())
            {
                LoggingData =
                    managerDbEntities.Loggings.OrderByDescending(logging => logging.Id)
                        .ToList();

                LoggingHeader = LoggingHeaderConstant + " ( " + LoggingData.Count + " )";

                JobHistoryData =
                    managerDbEntities.JobHistories.OrderByDescending(history => history.Created)
                        .ToList();

                JobHistoryHeader = JobHistoryHeaderConstant + " ( " + JobHistoryData.Count + " )";
                
                JobHistoryDetailData =
                    managerDbEntities.JobHistoryDetails.OrderByDescending(history => history.Created)
                        .ToList();

                JobHistoryDetailHeader = JobHistoryDetailHeaderConstant + " ( " + JobHistoryDetailData.Count + " )";

                JobDefinitionData =
                    managerDbEntities.JobDefinitions
                        .ToList();

                JobDefinitionDataHeader = JobDefinitionHeaderConstant + " ( " + JobDefinitionData.Count + " )";

                WorkerNodesData  =
                    managerDbEntities.WorkerNodes
                        .ToList();

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
    }
}