using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Teleopti.Analytics.Etl.ConfigTool.Transformer;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job.MultipleDate;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.Control
{
    /// <summary>
    /// Interaction logic for ManualControl.xaml
    /// </summary>
    public partial class ManualControl : UserControl
    {
        private DataSourceValidCollection _dataSourceCollection;
        private IJobMultipleDate _jobMultipleDatePeriods;
        private IJob _currentJob;
    	private IBaseConfiguration _baseConfiguration;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public ManualControl()
        {
            InitializeComponent();
            if (isInDesignMode) return;

            try
            {
                _dataSourceCollection = new DataSourceValidCollection(true);
                comboBoxLogDataSource.DataContext = _dataSourceCollection;
                UpdateControls(null);
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
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
                if (comboBoxLogDataSource.SelectedIndex > -1)
                    return (int)comboBoxLogDataSource.SelectedValue;

                    return -1;
            }
        }

        internal void UpdateControls(IJob job)
        {
            stackPanelLogDataSource.IsEnabled = false;
            groupBoxInitial.IsEnabled = false;
            groupBoxQueueStats.IsEnabled = false;
            groupBoxAgentStats.IsEnabled = false;
            groupBoxSchedule.IsEnabled = false;
            groupBoxForecast.IsEnabled = false;

            if (job == null)
            {
                return;
            }
            _currentJob = job;

            ReadOnlyCollection<JobCategoryType> jobCategoryCollection = _currentJob.JobCategoryCollection;
            _jobMultipleDatePeriods = new JobMultipleDate(defaultTimeZone);

            stackPanelLogDataSource.IsEnabled = _currentJob.NeedsParameterDataSource;
            groupBoxInitial.IsEnabled = false;
            groupBoxQueueStats.IsEnabled = false;
            groupBoxAgentStats.IsEnabled = false;
            groupBoxSchedule.IsEnabled = false;
            groupBoxForecast.IsEnabled = false;
            
            foreach (JobCategoryType jobCategoryType in jobCategoryCollection)
            {
                switch (jobCategoryType)
                {
                    case JobCategoryType.Initial:
                        if (jobCategoryCollection.Count == 2 && jobCategoryCollection.Contains(JobCategoryType.DoNotNeedDatePeriod))
                        {
                            groupBoxInitial.IsEnabled = true;
                            if (fromDatePickerInitial.SelectedDate == null || toDatePickerInitial.SelectedDate == null)
                            {
                                fromDatePickerInitial.SelectedDate = DateTime.Now;
                                toDatePickerInitial.SelectedDate = DateTime.Now.AddYears(1);
                            }
                            _jobMultipleDatePeriods.Add(fromDatePickerInitial.SelectedDate.Value.Date,
                                        toDatePickerInitial.SelectedDate.Value.Date.AddDays(1),
                                        JobCategoryType.Initial);
                        }
                        break;
                    case JobCategoryType.AgentStatistics:
                        groupBoxAgentStats.IsEnabled = true;
                        if (fromDatePickerAgentStats.SelectedDate == null || toDatePickerAgentStats.SelectedDate == null)
                        {
                            fromDatePickerAgentStats.SelectedDate = DateTime.Now;
                            toDatePickerAgentStats.SelectedDate = DateTime.Now;
                        }
                        _jobMultipleDatePeriods.Add(fromDatePickerAgentStats.SelectedDate.Value.Date,
                                        toDatePickerAgentStats.SelectedDate.Value.Date.AddDays(1),
                                        JobCategoryType.AgentStatistics);
                        break;
                    case JobCategoryType.QueueStatistics:
                        groupBoxQueueStats.IsEnabled = true;
                        if (fromDatePickerQueueStats.SelectedDate == null || toDatePickerQueueStats.SelectedDate == null)
                        {
                            fromDatePickerQueueStats.SelectedDate = DateTime.Now;
                            toDatePickerQueueStats.SelectedDate = DateTime.Now;
                        }
                        _jobMultipleDatePeriods.Add(fromDatePickerQueueStats.SelectedDate.Value.Date,
                                        toDatePickerQueueStats.SelectedDate.Value.Date.AddDays(1),
                                        JobCategoryType.QueueStatistics);
                        break;
                    case JobCategoryType.Schedule:
                        groupBoxSchedule.IsEnabled = true;
                        if (fromDatePickerSchedule.SelectedDate == null || toDatePickerSchedule.SelectedDate == null)
                        {
                            fromDatePickerSchedule.SelectedDate = DateTime.Now;
                            toDatePickerSchedule.SelectedDate = DateTime.Now;
                        }
                        _jobMultipleDatePeriods.Add(fromDatePickerSchedule.SelectedDate.Value.Date,
                                        toDatePickerSchedule.SelectedDate.Value.Date.AddDays(1),
                                        JobCategoryType.Schedule);
                        break;
                    case JobCategoryType.Forecast:
                        groupBoxForecast.IsEnabled = true;
                        if (fromDatePickerForecast.SelectedDate == null || toDatePickerForecast.SelectedDate == null)
                        {
                            fromDatePickerForecast.SelectedDate = DateTime.Now;
                            toDatePickerForecast.SelectedDate = DateTime.Now;
                        }
                        _jobMultipleDatePeriods.Add(fromDatePickerForecast.SelectedDate.Value.Date,
                                        toDatePickerForecast.SelectedDate.Value.Date.AddDays(1),
                                        JobCategoryType.Forecast);
                        break;
                    default:
                        break;
                }
            }
			if (_currentJob.Name == "Intraday")
				groupBoxSchedule.IsEnabled = false;
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
            comboBoxLogDataSource.DataContext = _dataSourceCollection = new DataSourceValidCollection(true);
        }

    	public void SetBaseConfiguration(IBaseConfiguration baseConfiguration)
    	{
    		_baseConfiguration = baseConfiguration;
    	}
    }
}
