using System;
using System.Configuration;
using System.Windows;
using Autofac;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.ConfigTool.Code.Gui.DataSourceConfiguration;
using Teleopti.Analytics.Etl.ConfigTool.Gui.DataSourceConfiguration;
using Teleopti.Analytics.Etl.ConfigTool.Transformer;

namespace Teleopti.Analytics.Etl.ConfigTool
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : IDisposable
	{
		private GeneralFunctions _generalFunctions;
		private DataSourceConfigurationView _dataSourceConfigurationDialog;
		private bool _isEtlToolLoading;
		private IJob _initialJob;
		private readonly IBaseConfiguration _baseConfiguration;

		public MainWindow(IBaseConfiguration baseConfiguration)
		{
			_baseConfiguration = baseConfiguration;
			InitializeComponent();
			_isEtlToolLoading = true;
			_generalFunctions = new GeneralFunctions(startConnectionString, new BaseConfigurationRepository());
			loadDataSources();
			manualEtl.InitialJobNowAvailable += manualEtl_InitialJobNowAvailable;
			manualEtl.JobStartedRunning += manualEtl_JobStartedRunning;
			manualEtl.JobStoppedRunning += manualEtl_JobStoppedRunning;

			scheduleControl.SetBaseConfiguration(baseConfiguration);
			manualEtl.SetBaseConfiguration(baseConfiguration);
			jobHistory.SetBaseConfiguration(baseConfiguration);

			DataContext = this;
			CanRun = true;
		}

		void manualEtl_JobStoppedRunning(object sender, AlarmEventArgs e)
		{
			menuItemDataSourceConfiguration.IsEnabled = true;
		}

		void manualEtl_JobStartedRunning(object sender, AlarmEventArgs e)
		{
			menuItemDataSourceConfiguration.IsEnabled = false;
		}

		void _timeZoneConfigurationForm_TimeToStartInitialLoad(object sender, AlarmEventArgs e)
		{
			manualEtl.RunJob(e.Job);
		}

		void manualEtl_InitialJobNowAvailable(object sender, AlarmEventArgs e)
		{
			_isEtlToolLoading = false;
			_initialJob = e.Job;
			if (_dataSourceConfigurationDialog != null)
				_dataSourceConfigurationDialog.EtlToolIsNowReady(_initialJob);
		}

		private void loadDataSources()
		{
			_generalFunctions.LoadNewDataSources();
		}

		public bool CanRun
		{
			get { return (bool)GetValue(CanRunProperty); }
			set { SetValue(CanRunProperty, value); }
		}

		public static readonly DependencyProperty CanRunProperty =
			 DependencyProperty.Register("CanRun", typeof(bool), typeof(MainWindow), new UIPropertyMetadata(false));

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			manualEtl.ManualControl.RegisterInitialConfigSetup(initializeDataSourceDialog);
			scheduleControl.SetTreeControl(manualEtl.myTree);
		}

		private bool initializeDataSourceDialog(string connectionString)
		{
			_generalFunctions = new GeneralFunctions(connectionString, new BaseConfigurationRepository());
			loadDataSources();
			_dataSourceConfigurationDialog = new DataSourceConfigurationView(new DataSourceConfigurationModel(_generalFunctions, _baseConfiguration), connectionString);
			_dataSourceConfigurationDialog.TimeToStartInitialLoad += _timeZoneConfigurationForm_TimeToStartInitialLoad;
			_dataSourceConfigurationDialog.Initialize();
			return true;
		}

		private void showDataSourceDialog()
		{
			if (_dataSourceConfigurationDialog.ShowForm)
			{
				_dataSourceConfigurationDialog.IsEtlToolLoading = _isEtlToolLoading;
				if (!_dataSourceConfigurationDialog.IsEtlToolLoading)
				{
					// ETL Tool tree is loaded with all jobs - inject Initial job to Data Source config view
					_dataSourceConfigurationDialog.EtlToolIsNowReady(_initialJob);
				}
				_dataSourceConfigurationDialog.ShowDialog();

				if (_dataSourceConfigurationDialog.CloseApplication)
				{
					_dataSourceConfigurationDialog.DetachEvent();
					_dataSourceConfigurationDialog = null;
					Close();
				}
				else
				{
					if (_dataSourceConfigurationDialog.IsSaved)
					{
						// Reload data sources and set enable state on jobs in treeview
						manualEtl.ReloadDataSources();
					}
					_dataSourceConfigurationDialog.DetachEvent();
					_dataSourceConfigurationDialog = null;
				}
			}
			else
			{
				if (_dataSourceConfigurationDialog.CloseApplication)
				{
					// don´t show DS config GUI and close app
					_dataSourceConfigurationDialog = null;
					Close();
				}
			}
		}

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			_baseConfiguration.JobHelper.SelectDataSourceContainer(manualEtl.myControl.TenantName);
			initializeDataSourceDialog(_baseConfiguration.JobHelper.SelectedDataSource.Statistic.ConnectionString);
			showDataSourceDialog();
		}

		private static string startConnectionString
		{
			get { return ConfigurationManager.AppSettings["datamartConnectionString"]; }
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_dataSourceConfigurationDialog != null)
				{
					_dataSourceConfigurationDialog.Dispose();
					_dataSourceConfigurationDialog = null;
				}
			}
		}
	}
}