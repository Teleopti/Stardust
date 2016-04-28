using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using Manager.Integration.Test.Data;
using Manager.Integration.Test.Helpers;
using Manager.Integration.Test.Models;
using Manager.Integration.Test.Tasks;
using Manager.Integration.Test.TestParams;
using Manager.Integration.Test.Timers;
using Manager.Integration.Test.WPF.Annotations;
using Manager.Integration.Test.WPF.Commands;
using Manager.Integration.Test.WPF.HttpListeners.Fiddler;
using Manager.Integration.Test.WPF.Properties;
using Manager.IntegrationTest.Console.Host.Helpers;
using Newtonsoft.Json;
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

		private ClearLoggingTableInDatabaseCommand _clearLoggingTableInDatabaseCommand;
		private CreateNewJobCommand _create20NewJobCommand;
		private CreateNewJobCommand _createNewJobCommand;

		private int _durationPerMessage = 2;
		private ObservableCollection<Logging> _errorLoggings;

		private string _errorLoggingHeader;

		private string _httpTrafficListenerHeader = HttpTrafficListenerHeaderConstant;
		private InstallCertificateCommand _installCertificateCommand;
		private bool _isConsoleHostStarted;
		private ObservableCollection<JobQueueItem> _jobDefinitionData;
		private string _jobDefinitionDataHeader;

		private ObservableCollection<Job> _jobs;
		private ListCollectionView _jobHistoryDataGroupBySentToData;
		private ObservableCollection<JobDetail> _jobHistoryDetailData;
		private string _jobHistoryDetailHeader;
		private string _jobHistoryGroupBySentToHeader;
		private string _jobHistoryHeader;
		private ObservableCollection<Logging> _loggings;
		private string _loggingHeader;

		private int _numberOfManagers;

		private int _numberOfMessages = 100;
		private int _numberOfNodes;
		private ObservableCollection<PerformanceTest> _performanceTests;
		private string _performanceTestHeader;
		private bool _refreshEnabled;
		private int _refreshProgressValue;
		private ShutDownHostCommand _shutDownHostCommand;
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
		private ObservableCollection<WorkerNode> _workerNodesData;

		public MainWindowViewModel()
		{
			ManagerDBConnectionString =
				ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString;

			ManagerDbRepository =
				new ManagerDbRepository(ManagerDBConnectionString);

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

			ClearLoggingTableInDatabaseCommand = new ClearLoggingTableInDatabaseCommand(this);

			ClearManagerTablesInDatabaseCommand = new ClearManagerTablesInDatabaseCommand(this);

			ToggleRefreshCommand = new ToggleRefreshCommand(this);

			CreateNewJobCommand = new CreateNewJobCommand();
			Create20NewJobCommand = new CreateNewJobCommand(20);

			StartUpNewManagerCommand = new StartUpNewManagerCommand();
			StartUpNewNodeCommand = new StartUpNewNodeCommand();

			StartDurationTestCommand = new StartDurationTestCommand(this);

			RefreshTimer = new Timer(5000);

			RefreshTimer.Elapsed += RefreshTimerOnElapsed;

			RefreshTimer.Start();

			RefreshEnabled = true;
		}

		public ManagerDbRepository ManagerDbRepository { get; set; }

		public string ManagerDBConnectionString { get; private set; }

		public StartDurationTestCommand StartDurationTestCommand { get; set; }

		public ClearManagerTablesInDatabaseCommand ClearManagerTablesInDatabaseCommand { get; set; }

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

		public ClearLoggingTableInDatabaseCommand ClearLoggingTableInDatabaseCommand
		{
			get { return _clearLoggingTableInDatabaseCommand; }
			set
			{
				_clearLoggingTableInDatabaseCommand = value;

				OnPropertyChanged();
			}
		}

		public ObservableCollection<Logging> Loggings
		{
			get { return _loggings; }
			set
			{
				_loggings = value;

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

		public ObservableCollection<WorkerNode> WorkerNodesData
		{
			get { return _workerNodesData; }
			set
			{
				_workerNodesData = value;

				OnPropertyChanged();
			}
		}

		public ObservableCollection<JobQueueItem> JobDefinitionData
		{
			get { return _jobDefinitionData; }
			set
			{
				_jobDefinitionData = value;

				OnPropertyChanged();
			}
		}

		public ObservableCollection<JobDetail> JobHistoryDetailData
		{
			get { return _jobHistoryDetailData; }

			set
			{
				_jobHistoryDetailData = value;

				OnPropertyChanged();
			}
		}

		public ObservableCollection<FiddlerCaptureInformation> FiddlerCaptureInformation { get; set; }

		public ObservableCollection<PerformanceTest> PerformanceTests
		{
			get { return _performanceTests; }

			set
			{
				_performanceTests = value;

				OnPropertyChanged();
			}
		}


		public ObservableCollection<Job> Jobs
		{
			get { return _jobs; }

			set
			{
				_jobs = value;

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


		public ObservableCollection<Logging> ErrorLoggings
		{
			get { return _errorLoggings; }
			set
			{
				_errorLoggings = value;

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

		public bool IsConsoleHostStarted
		{
			get { return _isConsoleHostStarted; }

			set
			{
				_isConsoleHostStarted = value;

				OnPropertyChanged();
			}
		}

		public int NumberOfMessages
		{
			get { return _numberOfMessages; }
			set
			{
				_numberOfMessages = value;

				OnPropertyChanged();
			}
		}

		public int DurationPerMessage
		{
			get { return _durationPerMessage; }
			set
			{
				_durationPerMessage = value;

				OnPropertyChanged();
			}
		}

		private CancellationTokenSource CancelTaskStartDurationTest { get; set; }


		private Task TaskStartDurationTest { get; set; }

		public void Dispose()
		{
			if (CancelTaskStartDurationTest != null)
			{
				CancelTaskStartDurationTest.Cancel();
			}

			if (TaskStartDurationTest != null)
			{
				TaskStartDurationTest.Dispose();
			}

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
					(Action) delegate
					{
						FiddlerCaptureInformation.Add(fiddlerCaptureInformation);
					});
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
			Task.Factory.StartNew(() =>
			{
				Application.Current.Dispatcher.Invoke(
					DispatcherPriority.Normal,
					(Action)delegate
					{
						RefreshProgressValue = 0;

						Status = "Refresh started...";

						PerformanceTests =
							ManagerDbRepository.PerformanceTests;

						PerformanceTestHeader =
							PerformanceTestHeaderConstant + " ( " + PerformanceTests.Count + " )";

						Loggings =
							ManagerDbRepository.Loggings;

						ErrorLoggings = new ObservableCollection<Logging>();

						if (Loggings != null && Loggings.Any())
						{
							Task.Factory.StartNew(() =>
							{
								foreach (var log in
									Loggings.Where(logging => logging.Level.Equals("ERROR", StringComparison.InvariantCultureIgnoreCase) ||
															  logging.Level.Equals("FATAL", StringComparison.InvariantCultureIgnoreCase)))
								{
									ErrorLoggings.Add(log);
								}
							});
						}

						ErrorLoggingHeader = ErrorLoggingHeaderConstant + " ( " + ErrorLoggings.Count + " )";

						++RefreshProgressValue;

						LoggingHeader = LoggingHeaderConstant + " ( " + Loggings.Count + " )";

						Jobs =
							ManagerDbRepository.Jobs;

						JobHistoryDataGroupBySentToData = new ListCollectionView(Jobs);

						if (JobHistoryDataGroupBySentToData.GroupDescriptions != null)
						{
							JobHistoryDataGroupBySentToData.GroupDescriptions.Add(new PropertyGroupDescription("SentToWorkerNodeUri"));
						}

						++RefreshProgressValue;

						JobHistoryHeader =
							JobHistoryHeaderConstant + " ( " + Jobs.Count + " )";

						JobHistoryGroupBySentToHeader =
							JobHistoryGroupBySentToHeaderConstant + " ( " + Jobs.Count + " )";

						JobHistoryDetailData =
							ManagerDbRepository.JobDetails;

						++RefreshProgressValue;

						JobHistoryDetailHeader =
							JobHistoryDetailHeaderConstant + " ( " + JobHistoryDetailData.Count + " )";

						JobDefinitionData =
							ManagerDbRepository.JobQueueItems;

						++RefreshProgressValue;

						JobDefinitionDataHeader =
							JobDefinitionHeaderConstant + " ( " + JobDefinitionData.Count + " )";

						WorkerNodesData =
							ManagerDbRepository.WorkerNodes;

						++RefreshProgressValue;

						WorkerNodeHeader =
							WorkerNodeHeaderConstant + " ( " + WorkerNodesData.Count + " )";

						Status = "Refresh finished.";

					});
			});

		}

		public bool DatabaseContainsInformation()
		{
			return true;
		}

		public void DatabaseClearAllInformation()
		{
			Task.Factory.StartNew(() =>
			{
				ManagerDbRepository.TruncateLoggingTable();

				GetData();
			});
		}

		public void ToggleRefresh()
		{
			RefreshEnabled = !RefreshEnabled;
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

		public void ClearAllManagerTablesInDatabase()
		{
			Task.Factory.StartNew(() =>
			{
				var managerDbRepository =
					new ManagerDbRepository(ManagerDBConnectionString);

				managerDbRepository.TruncateJobDetailTable();
				managerDbRepository.TruncateJobTable();
				managerDbRepository.TruncateJobQueueTable();
				managerDbRepository.TruncateWorkerNodeTable();

				GetData();
			});
		}

		public void StartDurationTest()
		{
			Task.Factory.StartNew(() =>
			{
				var mangerUriBuilder = new ManagerUriBuilder();
				var addToJobQueueUri = mangerUriBuilder.GetAddToJobQueueUri();

				var httpSender = new HttpSender();

				CancelTaskStartDurationTest = new CancellationTokenSource();

				List<Task<HttpResponseMessage>> tasks = new List<Task<HttpResponseMessage>>();

				Random random = new Random();

				for (int i = 0; i < 5000; i++)
				{
					var i1 = i;
					
					var randomTimeSpan = random.Next(5, 120);

					tasks.Add(new Task<HttpResponseMessage>(() =>
						{
							var testJobTimerParams =
							new TestJobTimerParams("Test job name " + i1, 
													TimeSpan.FromSeconds(randomTimeSpan));

						var jobParamsToJson = 
							JsonConvert.SerializeObject(testJobTimerParams);

							var jobQueueItem = new JobQueueItem
							{
							Name = "Job Name " + i1,
								Serialized = jobParamsToJson,
								Type = "NodeTest.JobHandlers.TestJobTimerParams",
								CreatedBy = "WPF Client"
							};

						return httpSender.PostAsync(addToJobQueueUri, 
													jobQueueItem).Result;

					}, CancelTaskStartDurationTest.Token));
							}

				var sendJobEveryDurationTimer=
					new SendJobEveryDurationTimer<HttpResponseMessage>(tasks,TimeSpan.FromSeconds(5));

				sendJobEveryDurationTimer.SendTimer.Start();
			});
		}
	}
}