﻿using System;
using System.Configuration;
using System.Windows;
using Autofac;
using Teleopti.Analytics.Etl.ConfigTool.Gui.DataSourceConfiguration;
using Teleopti.Analytics.Etl.ConfigTool.Transformer;
using Teleopti.Analytics.Etl.ConfigToolCode.Gui.DataSourceConfiguration;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer;

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
		private readonly IContainer _container;

		public MainWindow(IBaseConfiguration baseConfiguration, IContainer container)
		{
			_baseConfiguration = baseConfiguration;
			_container = container;
			InitializeComponent();
			_isEtlToolLoading = true;
			_generalFunctions = new GeneralFunctions(startConnectionString);
			loadDataSources();
			manualEtl.InitialJobNowAvailable += manualEtl_InitialJobNowAvailable;
			manualEtl.JobStartedRunning += manualEtl_JobStartedRunning;
			manualEtl.JobStoppedRunning += manualEtl_JobStoppedRunning;

			scheduleControl.SetBaseConfiguration(baseConfiguration);
			manualEtl.SetBaseConfiguration(baseConfiguration, _container);
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
			initializeDataSourceDialog(startConnectionString);

			// why not use:
			//_generalFunctions.DataSourceInvalidList
			var dataSourceInvalidCollection = new DataSourceInvalidCollection(startConnectionString);
			var dataSourceValidCollection = new DataSourceValidCollection(false, startConnectionString);
			
			if (dataSourceInvalidCollection.Count > 0 || (dataSourceValidCollection.Count + dataSourceInvalidCollection.Count == 0))
			{
				// Show config GUI if not ds at all or at least one invalid ds.
				CanRun = false;
				showDataSourceDialog();
				CanRun = true;
			}
			else
			{
				_dataSourceConfigurationDialog.DetachEvent();
				_dataSourceConfigurationDialog = null;
			}

			scheduleControl.SetTreeControl(manualEtl.myTree, _container);
		}

		private void initializeDataSourceDialog(string connectionString)
		{
			_generalFunctions = new GeneralFunctions(connectionString);
			loadDataSources();
			_dataSourceConfigurationDialog = new DataSourceConfigurationView(new DataSourceConfigurationModel(_generalFunctions, _baseConfiguration), connectionString);
			_dataSourceConfigurationDialog.TimeToStartInitialLoad += _timeZoneConfigurationForm_TimeToStartInitialLoad;
			_dataSourceConfigurationDialog.Initialize();
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
			initializeDataSourceDialog(_baseConfiguration.JobHelper.SelectedDataSourceContainer.DataSource.Statistic.ConnectionString);
			showDataSourceDialog();
		}

		private static string startConnectionString
		{
			get { return ConfigurationManager.AppSettings["datamartConnectionString"]; }
		}

		private TimeZoneInfo defaultTimeZone
		{
			get
			{
				return TimeZoneInfo.FindSystemTimeZoneById(_baseConfiguration.TimeZoneCode);
			}
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