using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Data;
using Manager.Integration.Test.WPF.Annotations;
using Manager.Integration.Test.WPF.Commands;

namespace Manager.Integration.Test.WPF.ViewModels
{
	public class MainWindowViewModel : INotifyPropertyChanged
	{
		private const string WorkerNodeHeaderConstant = "Worker Nodes";

		private const string ErrorLoggingHeaderConstant = "Logging errors";
		private const string LoggingHeaderConstant = "Logging";
		private const string JobHistoryHeaderConstant = "Jobs";
		private const string JobHistoryGroupBySentToHeaderConstant = "Job Group By Worker Nodes";
		private const string JobHistoryDetailHeaderConstant = "Job Details";
		private const string JobDefinitionHeaderConstant = "Job Queue";

		private const string PerformanceTestHeaderConstant = "Performance tests";

		private ClearDatabaseCommand _clearDatabaseCommand;
		private List<Logging> _errorLoggingData;

		private string _errorLoggingHeader;
		private List<JobQueue> _jobDefinitionData;
		private string _jobDefinitionDataHeader;

		private List<Job> _jobHistoryData;
		private ListCollectionView _jobHistoryDataGroupBySentToData;
		private List<JobDetail> _jobHistoryDetailData;
		private string _jobHistoryDetailHeader;
		private string _jobHistoryGroupBySentToHeader;
		private string _jobHistoryHeader;
		private List<Logging> _loggingData;
		private string _loggingHeader;
		private bool _refreshEnabled;
		private int _refreshProgressValue;

		private string _status;
		private ToggleRefreshCommand _toggleRefreshCommand;
		private string _toggleRefreshStatus;
		private string _workerNodeHeader;
		private List<WorkerNode> _workerNodesData;
		private List<PerformanceTest> _performanceTestData;
		private string _performanceTestHeader;

		public MainWindowViewModel()
		{
			GetData();

			ClearDatabaseCommand = new ClearDatabaseCommand(this);
			ToggleRefreshCommand = new ToggleRefreshCommand(this);

			CreateNewJobCommand = new CreateNewJobCommand();

			RefreshTimer = new Timer(5000);

			RefreshTimer.Elapsed += RefreshTimerOnElapsed;

			RefreshTimer.Start();

			RefreshEnabled = true;
		}

		public CreateNewJobCommand CreateNewJobCommand { get; set; }

		public string WorkerNodeHeader
		{
			get { return _workerNodeHeader; }
			set
			{
				_workerNodeHeader = value;

				OnPropertyChanged();
			}
		}

		public string Status
		{
			get { return _status; }
			set
			{
				_status = value;

				OnPropertyChanged();
			}
		}

		public Timer RefreshTimer { get; set; }

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

		public string JobHistoryGroupBySentToHeader
		{
			get { return _jobHistoryGroupBySentToHeader; }
			set
			{
				_jobHistoryGroupBySentToHeader = value;

				OnPropertyChanged();
			}
		}

		public ListCollectionView JobHistoryDataGroupBySentToData
		{
			get { return _jobHistoryDataGroupBySentToData; }

			set
			{
				_jobHistoryDataGroupBySentToData = value;

				OnPropertyChanged();
			}
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

		public List<JobQueue> JobDefinitionData
		{
			get { return _jobDefinitionData; }
			set
			{
				_jobDefinitionData = value;

				OnPropertyChanged();
			}
		}

		public List<JobDetail> JobHistoryDetailData
		{
			get { return _jobHistoryDetailData; }

			set
			{
				_jobHistoryDetailData = value;

				OnPropertyChanged();
			}
		}

		public List<PerformanceTest> PerformanceTestData
		{
			get { return _performanceTestData; }

			set
			{
				_performanceTestData = value;

				OnPropertyChanged();
			}
		}


		public List<Job> JobHistoryData
		{
			get { return _jobHistoryData; }

			set
			{
				_jobHistoryData = value;

				OnPropertyChanged();
			}
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

		public string PerformanceTestHeader
		{
			get { return _performanceTestHeader; }

			set
			{
				_performanceTestHeader = value;

				OnPropertyChanged();
			}
		}

		public string ErrorLoggingHeader
		{
			get { return _errorLoggingHeader; }
			set
			{
				_errorLoggingHeader = value;

				OnPropertyChanged();
			}
		}


		public List<Logging> ErrorLoggingData
		{
			get { return _errorLoggingData; }
			set
			{
				_errorLoggingData = value;

				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

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

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this,
				                new PropertyChangedEventArgs(propertyName));
			}
		}

		private void GetData()
		{
			RefreshProgressValue = 0;

			Status = "Refresh started...";

			using (var managerDbEntities = new ManagerDbEntities())
			{
				PerformanceTestData = 
					managerDbEntities.PerformanceTests.OrderByDescending(test => test.Id).ToList();

				PerformanceTestHeader= 
					PerformanceTestHeaderConstant  +" ( " + PerformanceTestData.Count + " )";

				LoggingData =
					managerDbEntities.Loggings.OrderByDescending(logging => logging.Id)
						.ToList();

				ErrorLoggingData =
					managerDbEntities.Loggings.Where(logging => !string.IsNullOrEmpty(logging.Exception)).ToList();

				ErrorLoggingHeader = ErrorLoggingHeaderConstant + " ( " + ErrorLoggingData.Count + " )";

				++RefreshProgressValue;

				LoggingHeader = LoggingHeaderConstant + " ( " + LoggingData.Count + " )";

				JobHistoryData =
					managerDbEntities.Jobs.OrderByDescending(history => history.Created)
						.ToList();

				JobHistoryDataGroupBySentToData = new ListCollectionView(JobHistoryData);

				if (JobHistoryDataGroupBySentToData.GroupDescriptions != null)
				{
					JobHistoryDataGroupBySentToData.GroupDescriptions.Add(new PropertyGroupDescription("SentTo"));
				}

				++RefreshProgressValue;

				JobHistoryHeader =
					JobHistoryHeaderConstant + " ( " + JobHistoryData.Count + " )";

				JobHistoryGroupBySentToHeader =
					JobHistoryGroupBySentToHeaderConstant + " ( " + JobHistoryData.Count + " )";

				JobHistoryDetailData =
					managerDbEntities.JobDetails.OrderByDescending(history => history.Created)
						.ToList();

				++RefreshProgressValue;

				JobHistoryDetailHeader =
					JobHistoryDetailHeaderConstant + " ( " + JobHistoryDetailData.Count + " )";

				JobDefinitionData =
					managerDbEntities.JobQueues
						.ToList();

				++RefreshProgressValue;

				JobDefinitionDataHeader =
					JobDefinitionHeaderConstant + " ( " + JobDefinitionData.Count + " )";

				WorkerNodesData =
					managerDbEntities.WorkerNodes
						.ToList();

				++RefreshProgressValue;

				WorkerNodeHeader =
					WorkerNodeHeaderConstant + " ( " + WorkerNodesData.Count + " )";
			}

			Status = "Refresh finished.";
		}

		public bool DatabaseContainsInformation()
		{
			return true;
		}

		public void DatabaseClearAllInformation()
		{
			using (var managerDbEntities = new ManagerDbEntities())
			{
				managerDbEntities.Database.ExecuteSqlCommand("TRUNCATE TABLE Stardust.Logging");
			}

			GetData();
		}

		public void ToggleRefresh()
		{
			RefreshEnabled = !RefreshEnabled;
		}
	}
}