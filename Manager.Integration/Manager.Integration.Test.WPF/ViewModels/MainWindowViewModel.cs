using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using Manager.Integration.Test.Tasks;
using Manager.Integration.Test.WPF.Annotations;
using Manager.Integration.Test.WPF.Commands;
using Manager.Integration.Test.WPF.HttpListeners.Fiddler;
using Manager.Integration.Test.WPF.Properties;
using Timer = System.Timers.Timer;

namespace Manager.Integration.Test.WPF.ViewModels
{
	public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
	{
		private const string WorkerNodeHeaderConstant = "Worker Nodes";

		private const string ErrorLoggingHeaderConstant = "Logging errors";
		private const string LoggingHeaderConstant = "Logging";
		private const string JobHistoryHeaderConstant = "Jobs";
		private const string JobHistoryGroupBySentToHeaderConstant = "Job Group By Worker Nodes";
		private const string JobHistoryDetailHeaderConstant = "Job Details";
		private const string JobDefinitionHeaderConstant = "Job Queue";

		private const string PerformanceTestHeaderConstant = "Performance tests";

		private const string HttpTrafficListenerHeaderConstant = "HTTP Traffic";

		private ClearDatabaseCommand _clearDatabaseCommand;
		private CreateNewJobCommand _create20NewJobCommand;
		private CreateNewJobCommand _createNewJobCommand;
		private List<Logging> _errorLoggingData;

		private string _errorLoggingHeader;

		private string _httpTrafficListenerHeader = HttpTrafficListenerHeaderConstant;
		private InstallCertificateCommand _installCertificateCommand;
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

		private int _numberOfManagers;
		private int _numberOfNodes;
		private List<PerformanceTest> _performanceTestData;
		private string _performanceTestHeader;
		private bool _refreshEnabled;
		private int _refreshProgressValue;
		private StartFiddlerCaptureCommand _startFiddlerCaptureCommand;
		private StartHostCommand _startHostCommand;
		private StartUpNewManagerCommand _startUpNewManagerCommand;
		private StartUpNewNodeCommand _startUpNewNodeCommand;

		private string _status;
		private StopFiddlerCaptureCommand _stopFiddlerCaptureCommand;
		private ToggleRefreshCommand _toggleRefreshCommand;
		private string _toggleRefreshStatus;
		private UnInstallCertificateCommand _unInstallCertificateCommand;
		private string _workerNodeHeader;
		private List<WorkerNode> _workerNodesData;
		private ShutDownHostCommand _shutDownHostCommand;
		private bool _isConsoleHostStarted;

		public MainWindowViewModel()
		{
			//---------------------------------------
			// Set up Fiddler.
			//---------------------------------------
			FiddlerCaptureUrlConfiguration = new FiddlerCaptureUrlConfiguration();

			FiddlerCapture = new FiddlerCapture(FiddlerCaptureUrlConfiguration);

			FiddlerCapture.NewDataCapturedEventHandler += NewDataCapturedEventHandler;

			StartFiddlerCaptureCommand = new StartFiddlerCaptureCommand(FiddlerCapture);

			StopFiddlerCaptureCommand = new StopFiddlerCaptureCommand(FiddlerCapture);

			FiddlerCaptureInformation = new ObservableCollection<FiddlerCaptureInformation>();

			InstallCertificateCommand = new InstallCertificateCommand();

			UnInstallCertificateCommand = new UnInstallCertificateCommand();

			//---------------------------------------
			// Manager console host.
			//---------------------------------------
			StartHostCommand = new StartHostCommand(this);
			ShutDownHostCommand = new ShutDownHostCommand(this);
			
			NumberOfManagers = Settings.Default.NumberOfManagers;
			NumberOfNodes = Settings.Default.NumberOfNodes;

			IsConsoleHostStarted = false;

			//---------------------------------------
			// Do the rest.
			//---------------------------------------
			GetData();

			ClearDatabaseCommand = new ClearDatabaseCommand(this);
			ToggleRefreshCommand = new ToggleRefreshCommand(this);

			CreateNewJobCommand = new CreateNewJobCommand();
			Create20NewJobCommand = new CreateNewJobCommand(20);

			StartUpNewManagerCommand = new StartUpNewManagerCommand();
			StartUpNewNodeCommand = new StartUpNewNodeCommand();

			RefreshTimer = new Timer(5000);

			RefreshTimer.Elapsed += RefreshTimerOnElapsed;

			RefreshTimer.Start();

			RefreshEnabled = true;
		}

		public ShutDownHostCommand ShutDownHostCommand
		{
			get { return _shutDownHostCommand; }
			set
			{
				_shutDownHostCommand = value;

				OnPropertyChanged();
			}
		}

		public UnInstallCertificateCommand UnInstallCertificateCommand
		{
			get { return _unInstallCertificateCommand; }
			set
			{
				_unInstallCertificateCommand = value;

				OnPropertyChanged();
			}
		}

		public InstallCertificateCommand InstallCertificateCommand
		{
			get { return _installCertificateCommand; }
			set
			{
				_installCertificateCommand = value;

				OnPropertyChanged();
			}
		}

		public StartHostCommand StartHostCommand
		{
			get { return _startHostCommand; }
			set
			{
				_startHostCommand = value;

				OnPropertyChanged();
			}
		}

		public FiddlerCapture FiddlerCapture { get; private set; }

		public FiddlerCaptureUrlConfiguration FiddlerCaptureUrlConfiguration { get; set; }

		public StartFiddlerCaptureCommand StartFiddlerCaptureCommand
		{
			get { return _startFiddlerCaptureCommand; }
			set
			{
				_startFiddlerCaptureCommand = value;

				OnPropertyChanged();
			}
		}

		public StopFiddlerCaptureCommand StopFiddlerCaptureCommand
		{
			get { return _stopFiddlerCaptureCommand; }
			set
			{
				_stopFiddlerCaptureCommand = value;

				OnPropertyChanged();
			}
		}

		public StartUpNewNodeCommand StartUpNewNodeCommand
		{
			get { return _startUpNewNodeCommand; }
			set
			{
				_startUpNewNodeCommand = value;

				OnPropertyChanged();
			}
		}

		public StartUpNewManagerCommand StartUpNewManagerCommand
		{
			get { return _startUpNewManagerCommand; }
			set
			{
				_startUpNewManagerCommand = value;

				OnPropertyChanged();
			}
		}

		public CreateNewJobCommand Create20NewJobCommand
		{
			get { return _create20NewJobCommand; }
			set
			{
				_create20NewJobCommand = value;

				OnPropertyChanged();
			}
		}

		public CreateNewJobCommand CreateNewJobCommand
		{
			get { return _createNewJobCommand; }
			set
			{
				_createNewJobCommand = value;

				OnPropertyChanged();
			}
		}

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

		public ObservableCollection<FiddlerCaptureInformation> FiddlerCaptureInformation { get; set; }

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

		public string HttpTrafficListenerHeader
		{
			get { return _httpTrafficListenerHeader; }

			set
			{
				_httpTrafficListenerHeader = value;

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

		public int NumberOfManagers
		{
			get { return _numberOfManagers; }

			set
			{
				_numberOfManagers = value;

				OnPropertyChanged();
			}
		}

		public int NumberOfNodes
		{
			get { return _numberOfNodes; }
			set
			{
				_numberOfNodes = value;

				OnPropertyChanged();
			}
		}

		private CancellationTokenSource CancellationTokenSourceAppDomainTask { get; set; }

		public AppDomainTask AppDomainTask { get; set; }

		public void Dispose()
		{
			if (CancellationTokenSourceAppDomainTask != null)
			{
				CancellationTokenSourceAppDomainTask.Cancel();
			}

			if (AppDomainTask != null)
			{
				AppDomainTask.Dispose();
			}

			if (FiddlerCapture != null)
			{
				FiddlerCapture.Dispose();
			}

			Settings.Default.NumberOfManagers = NumberOfManagers;
			Settings.Default.NumberOfNodes = NumberOfNodes;

			Settings.Default.Save();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void NewDataCapturedEventHandler(object sender, FiddlerCaptureInformation fiddlerCaptureInformation)
		{
			Task.Factory.StartNew(() =>
			{
				Application.Current.Dispatcher.Invoke(
					DispatcherPriority.Normal,
					(Action) delegate { FiddlerCaptureInformation.Add(fiddlerCaptureInformation); });
			});
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

				PerformanceTestHeader =
					PerformanceTestHeaderConstant + " ( " + PerformanceTestData.Count + " )";

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
					JobHistoryDataGroupBySentToData.GroupDescriptions.Add(new PropertyGroupDescription("SentToWorkerNodeUri"));
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
						.OrderBy(queue => queue.Created).ToList();

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
			Task.Factory.StartNew(() =>
			{
				using (var managerDbEntities = new ManagerDbEntities())
				{
					managerDbEntities.Database.ExecuteSqlCommand("TRUNCATE TABLE Stardust.Logging");
				}

				GetData();
			});
		}

		public void ToggleRefresh()
		{
			RefreshEnabled = !RefreshEnabled;
		}

		public bool IsConsoleHostStarted
		{
			get { return _isConsoleHostStarted; }

			set
			{
				_isConsoleHostStarted = value;

				OnPropertyChanged();
			}
		}


		public void StartConsoleHost()
		{
			Task.Factory.StartNew(() =>
			{
				CancellationTokenSourceAppDomainTask = new CancellationTokenSource();

				AppDomainTask = new AppDomainTask("Debug");

				var task = AppDomainTask.StartTask(NumberOfManagers,
				                                   NumberOfNodes,
				                                   useLoadBalancerIfJustOneManager: true,
				                                   cancellationTokenSource: CancellationTokenSourceAppDomainTask);

				IsConsoleHostStarted = true;
			});
		}

		public void ShutDownConsoleHost()
		{
			if (CancellationTokenSourceAppDomainTask != null)
			{
				CancellationTokenSourceAppDomainTask.Cancel();
			}

			if (AppDomainTask != null)
			{
				AppDomainTask.Dispose();
			}

			IsConsoleHostStarted = false;
		}
	}
}