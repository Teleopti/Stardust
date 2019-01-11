using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Autofac;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.ConfigTool.Transformer;
using Teleopti.Ccc.Domain.MultiTenancy;
using Color = System.Drawing.Color;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.Control
{
	public partial class ManualControl
	{
		private DataSourceValidCollection _dataSourceCollection;
		private IJobMultipleDate _jobMultipleDatePeriods;
		private IJob _currentJob;
		private IBaseConfiguration _baseConfiguration;
		private Func<string, bool> _callBackWithConnectionString;

		public ManualControl()
		{
			InitializeComponent();
			if (isInDesignMode) return;

			GroupBoxAgentStats.IsEnabledChanged += (o, s) => changeForegroundColor(o);
			GroupBoxForecast.IsEnabledChanged += (o, s) => changeForegroundColor(o);
			GroupBoxInitial.IsEnabledChanged += (o, s) => changeForegroundColor(o);
			GroupBoxQueueStats.IsEnabledChanged += (o, s) => changeForegroundColor(o);
			GroupBoxSchedule.IsEnabledChanged += (o, s) => changeForegroundColor(o);
		}

		public void SetTenantDataSource(IEnumerable<TenantInfo> source)
		{
			ComboBoxDataSource.DataContext = source;
		}

		private static void changeForegroundColor(object sender)
		{
			var box = sender as GroupBox;
			if (box == null) return;

			var boxPanel = box.Content as StackPanel;
			if (boxPanel == null) return;

			var color = box.IsEnabled
										? Color.Black
										: Color.Gray;
			var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.R, color.B));

			foreach (var datePicker in (from StackPanel pickerPanel in boxPanel.Children
												 select pickerPanel.Children[1]).OfType<System.Windows.Controls.Control>())
				datePicker.Foreground = brush;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
		private static void showErrorMessage(string message)
		{
			MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		private static bool isInDesignMode
		{
			get
			{
				var prop = DesignerProperties.IsInDesignModeProperty;
				return
						(bool)
						DependencyPropertyDescriptor.FromProperty(prop, typeof(FrameworkElement)).Metadata.DefaultValue;

			}
		}

		internal int LogDataSource
		{
			get
			{
				if (ComboBoxLogDataSource.SelectedIndex > -1)
					return (int)ComboBoxLogDataSource.SelectedValue;

				return -1;
			}
		}

		internal string TenantName
		{
			get
			{
				if (ComboBoxDataSource.SelectedIndex > -1)
					return (string)ComboBoxDataSource.SelectedValue;

				return null;
			}
		}


		internal void UpdateControls(IJob job)
		{
			StackPanelLogDataSource.IsEnabled = false;
			GroupBoxInitial.IsEnabled = false;
			GroupBoxQueueStats.IsEnabled = false;
			GroupBoxAgentStats.IsEnabled = false;
			GroupBoxSchedule.IsEnabled = false;
			GroupBoxForecast.IsEnabled = false;

			if (job == null)
			{
				return;
			}
			_currentJob = job;

			var jobCategoryCollection = _currentJob.JobCategoryCollection;
			_jobMultipleDatePeriods = new JobMultipleDate(defaultTimeZone);

			StackPanelLogDataSource.IsEnabled = _currentJob.NeedsParameterDataSource;
			GroupBoxInitial.IsEnabled = false;
			GroupBoxQueueStats.IsEnabled = false;
			GroupBoxAgentStats.IsEnabled = false;
			GroupBoxSchedule.IsEnabled = false;
			GroupBoxForecast.IsEnabled = false;


			foreach (JobCategoryType jobCategoryType in jobCategoryCollection)
			{
				switch (jobCategoryType)
				{
					case JobCategoryType.Initial:
						if (jobCategoryCollection.Count == 2 && jobCategoryCollection.Contains(JobCategoryType.DoNotNeedDatePeriod))
						{
							GroupBoxInitial.IsEnabled = true;
							if (FromDatePickerInitial.SelectedDate == null || ToDatePickerInitial.SelectedDate == null)
							{
								FromDatePickerInitial.SelectedDate = DateTime.Now;
								ToDatePickerInitial.SelectedDate = DateTime.Now.AddYears(1);
							}
							_jobMultipleDatePeriods.Add(FromDatePickerInitial.SelectedDate.Value.Date,
													ToDatePickerInitial.SelectedDate.Value.Date,
													JobCategoryType.Initial);
						}
						break;
					case JobCategoryType.AgentStatistics:
						GroupBoxAgentStats.IsEnabled = true;
						if (FromDatePickerAgentStats.SelectedDate == null || ToDatePickerAgentStats.SelectedDate == null)
						{
							FromDatePickerAgentStats.SelectedDate = DateTime.Now;
							ToDatePickerAgentStats.SelectedDate = DateTime.Now;
						}
						_jobMultipleDatePeriods.Add(FromDatePickerAgentStats.SelectedDate.Value.Date,
														ToDatePickerAgentStats.SelectedDate.Value.Date,
														JobCategoryType.AgentStatistics);
						break;
					case JobCategoryType.QueueStatistics:
						GroupBoxQueueStats.IsEnabled = true;
						if (FromDatePickerQueueStats.SelectedDate == null || ToDatePickerQueueStats.SelectedDate == null)
						{
							FromDatePickerQueueStats.SelectedDate = DateTime.Now;
							ToDatePickerQueueStats.SelectedDate = DateTime.Now;
						}
						_jobMultipleDatePeriods.Add(FromDatePickerQueueStats.SelectedDate.Value.Date,
														ToDatePickerQueueStats.SelectedDate.Value.Date,
														JobCategoryType.QueueStatistics);
						break;
					case JobCategoryType.Schedule:
						GroupBoxSchedule.IsEnabled = true;
						if (FromDatePickerSchedule.SelectedDate == null || ToDatePickerSchedule.SelectedDate == null)
						{
							FromDatePickerSchedule.SelectedDate = DateTime.Now;
							ToDatePickerSchedule.SelectedDate = DateTime.Now;
						}
						_jobMultipleDatePeriods.Add(FromDatePickerSchedule.SelectedDate.Value.Date,
														ToDatePickerSchedule.SelectedDate.Value.Date,
														JobCategoryType.Schedule);
						break;
					case JobCategoryType.Forecast:
						GroupBoxForecast.IsEnabled = true;
						if (FromDatePickerForecast.SelectedDate == null || ToDatePickerForecast.SelectedDate == null)
						{
							FromDatePickerForecast.SelectedDate = DateTime.Now;
							ToDatePickerForecast.SelectedDate = DateTime.Now;
						}
						_jobMultipleDatePeriods.Add(FromDatePickerForecast.SelectedDate.Value.Date,
														ToDatePickerForecast.SelectedDate.Value.Date,
														JobCategoryType.Forecast);
						break;
				}
			}
			if (_currentJob.Name != "Intraday")
				return;
			GroupBoxSchedule.IsEnabled = false;
			GroupBoxForecast.IsEnabled = false;
			GroupBoxAgentStats.IsEnabled = false;
			GroupBoxQueueStats.IsEnabled = false;
		}

		private TimeZoneInfo defaultTimeZone
		{
			get { return TimeZoneInfo.FindSystemTimeZoneById(_baseConfiguration.TimeZoneCode); }
		}

		internal IJobMultipleDate JobMultipleDatePeriods
		{
			get
			{
				UpdateControls(_currentJob);
				return _jobMultipleDatePeriods;
			}
		}

		internal void ReloadDataSourceComboBox()
		{

			var dataSource = App.Container.Resolve<ITenants>().DataSourceForTenant(((TenantInfo) ComboBoxDataSource.SelectedItem).Name);
			ComboBoxLogDataSource.DataContext = _dataSourceCollection = new DataSourceValidCollection(true, dataSource.Analytics.ConnectionString);
		}

		public void SetBaseConfiguration(IBaseConfiguration baseConfiguration)
		{
			_baseConfiguration = baseConfiguration;
		}

		private void ComboBoxDataSource_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				_baseConfiguration.JobHelper.SelectDataSourceContainer((string)ComboBoxDataSource.SelectedValue);
				_callBackWithConnectionString(_baseConfiguration.JobHelper.SelectedDataSource.Analytics.ConnectionString);

				_dataSourceCollection = new DataSourceValidCollection(true, _baseConfiguration.JobHelper.SelectedDataSource.Analytics.ConnectionString);
				ComboBoxLogDataSource.DataContext = _dataSourceCollection;

				UpdateControls(null);
				if (ComboBoxLogDataSource.Items.Count > 0)
					ComboBoxLogDataSource.SelectedIndex = 0;
			}
			catch (Exception ex)
			{
				string msg = string.Format(CultureInfo.InvariantCulture,
																	 "The ETL Tool cannot be started. Connection to the Analytics database failed. Check the settings in the config file. {0}{1}",
																	 "\n\n", ex.Message);
				showErrorMessage(msg);
				Environment.Exit(0);
			}
		}

		public void RegisterInitialConfigSetup(Func<string, bool> callBackWithConnectionString)
		{
			_callBackWithConnectionString = callBackWithConnectionString;
		}
	}
}
