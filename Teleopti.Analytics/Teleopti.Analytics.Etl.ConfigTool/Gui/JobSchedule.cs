using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Entity;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Service;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.ConfigTool.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui
{
	public partial class JobSchedule : Form
	{
		private readonly bool _isNewSchedule;
		private IEtlJobSchedule _etlJobSchedule;
		private readonly IEtlJobSchedule _etlJobScheduleToEdit;
		private readonly IJobScheduleRepository _repository;
		private bool _isScheduleSettingsValid;
		private bool _isOkButtonClicked;
		private IJob _selectedJob;
		private readonly ObservableCollection<IEtlJobSchedule> _observableCollection;
		private readonly IBaseConfiguration _baseConfiguration;
		private readonly bool _selectDataSourceIsPossible;

		public JobSchedule(IEtlJobSchedule etlJobSchedule, ObservableCollection<IEtlJobSchedule> observableCollection,
			IBaseConfiguration baseConfiguration, bool selectDataSourceIsPossible)
		{
			InitializeComponent();
			_repository = new JobScheduleRepository();
			_repository.SetDataMartConnectionString(ConfigurationManager.AppSettings["datamartConnectionString"]);
			_observableCollection = observableCollection;
			_baseConfiguration = baseConfiguration;
			_selectDataSourceIsPossible = selectDataSourceIsPossible;

			if (etlJobSchedule == null)
			{
				_isNewSchedule = true;
			}
			else
			{
				_etlJobSchedule = etlJobSchedule;
				_etlJobScheduleToEdit = etlJobSchedule;
			}
		}

		private void JobSchedule_Load(object sender, EventArgs e)
		{
			fillDataSourceCombo();
			fillJobCombo();
			updateScheduleControls();
		}

		private void fillJobCombo()
		{
			var pmInfoProvider = App.Container.Resolve<PmInfoProvider>();
			var container = new IocContainerHolder(App.Container);
			var jobCollection =
				new JobCollection(
					new JobParameters(
						null, 1,
						_baseConfiguration.TimeZoneCode,
						_baseConfiguration.IntervalLength.Value,
						pmInfoProvider.Cube(),
						pmInfoProvider.PmInstallation(),
						CultureInfo.CurrentCulture,
						container,
						_baseConfiguration.RunIndexMaintenance,
						_baseConfiguration.InsightsConfig?.IsValid() ?? false
					)
				);
			comboBoxJob.DataSource = jobCollection;
			comboBoxJob.DisplayMember = "Name";
			comboBoxJob.ValueMember = "Name";
		}

		private void fillDataSourceCombo()
		{
			var dataSourceCollection = (List<IDataSourceEtl>)new DataSourceValidCollection(true, ConfigurationManager.AppSettings["datamartConnectionString"]);
			if (dataSourceCollection.Any() && !_selectDataSourceIsPossible)
			{
				var temp = dataSourceCollection.First();
				dataSourceCollection.Clear();
				dataSourceCollection = new List<IDataSourceEtl> {temp};
			}
			comboBoxDataSource.DataSource = dataSourceCollection;
			comboBoxDataSource.DisplayMember = "DataSourceName";
			comboBoxDataSource.ValueMember = "DataSourceId";
		}

		private void updateScheduleControls()
		{
			if (_isNewSchedule)
			{
				radioButtonOccursOnce.Checked = true;
				radioButtonRelativePeriodInitial.Checked = true;
			}
			else
			{
				textBoxScheduleName.Text = _etlJobSchedule.ScheduleName;
				checkBoxEnabled.Checked = _etlJobSchedule.Enabled;
				comboBoxJob.SelectedValue = _etlJobSchedule.JobName;
				if (_etlJobSchedule.ScheduleType == JobScheduleType.OccursDaily)
				{
					radioButtonOccursOnce.Checked = true;
					radioButtonOccursEvery.Checked = false;
					dateTimePickerOccursOnce.Value = dateTimePickerOccursOnce.MinDate.AddMinutes(_etlJobSchedule.OccursOnceAt);
				}
				else
				{
					radioButtonOccursOnce.Checked = false;
					radioButtonOccursEvery.Checked = true;
					numericUpDownOccursEveryMinute.Value = _etlJobSchedule.OccursEveryMinute;
					dateTimePickerStartingAt.Value = dateTimePickerOccursOnce.MinDate.AddMinutes(_etlJobSchedule.OccursEveryMinuteStartingAt);
					dateTimePickerEndingAt.Value = dateTimePickerOccursOnce.MinDate.AddMinutes(_etlJobSchedule.OccursEveryMinuteEndingAt);
				}

				comboBoxDataSource.SelectedValue = _etlJobSchedule.DataSourceId;
				updateRelativePeriods();
			}
		}

		private void updateRelativePeriods()
		{
			foreach (IEtlJobRelativePeriod relativePeriod in _etlJobSchedule.RelativePeriodCollection)
			{
				switch (relativePeriod.JobCategory)
				{
					case JobCategoryType.Initial:
						numericUpDownRelativePeriodStartInitial.Value = relativePeriod.RelativePeriod.Minimum;
						numericUpDownRelativePeriodEndInitial.Value = relativePeriod.RelativePeriod.Maximum;
						if (relativePeriod.RelativePeriod.Minimum == 0 && relativePeriod.RelativePeriod.Maximum == 0)
							radioButtonRelativePeriodTodayInitial.Checked = true;
						else
							radioButtonRelativePeriodInitial.Checked = true;
						break;
					case JobCategoryType.QueueStatistics:
						numericUpDownRelativePeriodStartQueueStats.Value = relativePeriod.RelativePeriod.Minimum;
						numericUpDownRelativePeriodEndQueueStats.Value = relativePeriod.RelativePeriod.Maximum;
						if (relativePeriod.RelativePeriod.Minimum == 0 && relativePeriod.RelativePeriod.Maximum == 0)
							radioButtonRelativePeriodTodayQueueStats.Checked = true;
						else
							radioButtonRelativePeriodQueueStats.Checked = true;
						break;
					case JobCategoryType.AgentStatistics:
						numericUpDownRelativePeriodStartAgentStats.Value = relativePeriod.RelativePeriod.Minimum;
						numericUpDownRelativePeriodEndAgentStats.Value = relativePeriod.RelativePeriod.Maximum;
						if (relativePeriod.RelativePeriod.Minimum == 0 && relativePeriod.RelativePeriod.Maximum == 0)
							radioButtonRelativePeriodTodayAgentStats.Checked = true;
						else
							radioButtonRelativePeriodAgentStats.Checked = true;
						break;
					case JobCategoryType.Schedule:
						numericUpDownRelativePeriodStartSchedule.Value = relativePeriod.RelativePeriod.Minimum;
						numericUpDownRelativePeriodEndSchedule.Value = relativePeriod.RelativePeriod.Maximum;
						if (relativePeriod.RelativePeriod.Minimum == 0 && relativePeriod.RelativePeriod.Maximum == 0)
							radioButtonRelativePeriodTodaySchedule.Checked = true;
						else
							radioButtonRelativePeriodSchedule.Checked = true;
						break;
					case JobCategoryType.Forecast:
						numericUpDownRelativePeriodStartForecast.Value = relativePeriod.RelativePeriod.Minimum;
						numericUpDownRelativePeriodEndForecast.Value = relativePeriod.RelativePeriod.Maximum;
						if (relativePeriod.RelativePeriod.Minimum == 0 && relativePeriod.RelativePeriod.Maximum == 0)
							radioButtonRelativePeriodTodayForecast.Checked = true;
						else
							radioButtonRelativePeriodForecast.Checked = true;
						break;
					default:
						break;
				}
			}
		}

		private void dateTimePickerEndingAt_ValueChanged(object sender, EventArgs e)
		{
			if (dateTimePickerEndingAt.Value.TimeOfDay == TimeSpan.Zero)
			{
				// Do not allow ending time to be 00:00.
				dateTimePickerEndingAt.Value = dateTimePickerEndingAt.Value.Add(new TimeSpan(23, 59, 0));
			}
			schedulingChanged(sender, e);
		}

		private void schedulingChanged(object sender, EventArgs e)
		{
			if (radioButtonOccursOnce.Checked)
			{
				dateTimePickerOccursOnce.Enabled = true;
				numericUpDownOccursEveryMinute.Enabled = false;
				dateTimePickerStartingAt.Enabled = false;
				dateTimePickerEndingAt.Enabled = false;
			}
			else
			{
				dateTimePickerOccursOnce.Enabled = false;
				numericUpDownOccursEveryMinute.Enabled = true;
				dateTimePickerStartingAt.Enabled = true;
				dateTimePickerEndingAt.Enabled = true;
			}

			numericUpDownRelativePeriodStartInitial.Enabled = !radioButtonRelativePeriodTodayInitial.Checked;
			numericUpDownRelativePeriodEndInitial.Enabled = !radioButtonRelativePeriodTodayInitial.Checked;

			numericUpDownRelativePeriodStartQueueStats.Enabled = !radioButtonRelativePeriodTodayQueueStats.Checked;
			numericUpDownRelativePeriodEndQueueStats.Enabled = !radioButtonRelativePeriodTodayQueueStats.Checked;

			numericUpDownRelativePeriodStartAgentStats.Enabled = !radioButtonRelativePeriodTodayAgentStats.Checked;
			numericUpDownRelativePeriodEndAgentStats.Enabled = !radioButtonRelativePeriodTodayAgentStats.Checked;

			numericUpDownRelativePeriodStartSchedule.Enabled = !radioButtonRelativePeriodTodaySchedule.Checked;
			numericUpDownRelativePeriodEndSchedule.Enabled = !radioButtonRelativePeriodTodaySchedule.Checked;

			numericUpDownRelativePeriodStartForecast.Enabled = !radioButtonRelativePeriodTodayForecast.Checked;
			numericUpDownRelativePeriodEndForecast.Enabled = !radioButtonRelativePeriodTodayForecast.Checked;

			updateDescription();
		}

		private void setDefaultForRelativePeriods(IJob job)
		{
			groupBoxQueueStats.Enabled = false;
			groupBoxAgentStats.Enabled = false;
			groupBoxForecast.Enabled = false;
			groupBoxSchedule.Enabled = false;
			groupBoxInitial.Enabled = false;
			buttonOk.Enabled = true;

			switch (job.Name)
			{
				case "Initial":
					initiateInitialLoadDatePeriod();
					break;
				case "Schedule":
					initiateScheduleDatePeriod();
					break;
				case "Queue Statistics":
					initiateQueueStatisticsDatePeriod();
					break;
				case "Forecast":
					initiateForecastDatePeriod();
					break;
				case "Agent Statistics":
					initiateAgentStatisticsDatePeriod();
					break;
				case "Intraday":
					//					Do nothing, all date periods are remove from Intraday job
					break;
				case "Nightly":
					initiateScheduleDatePeriod();
					initiateQueueStatisticsDatePeriod();
					initiateAgentStatisticsDatePeriod();
					break;
				default:
					break;
			}
		}

		private void initiateInitialLoadDatePeriod()
		{
			groupBoxInitial.Enabled = true;
			numericUpDownRelativePeriodStartInitial.Value = 0;
			numericUpDownRelativePeriodEndInitial.Value = 365;
			radioButtonRelativePeriodInitial.Checked = true;
		}

		private void initiateAgentStatisticsDatePeriod()
		{
			groupBoxAgentStats.Enabled = true;
			radioButtonRelativePeriodTodayAgentStats.Checked = true;
			radioButtonRelativePeriodAgentStats.Checked = false;
		}

		private void initiateForecastDatePeriod()
		{
			groupBoxForecast.Enabled = true;
			numericUpDownRelativePeriodStartForecast.Value = -7;
			numericUpDownRelativePeriodEndForecast.Value = 31;
			radioButtonRelativePeriodForecast.Checked = true;
		}

		private void initiateQueueStatisticsDatePeriod()
		{
			groupBoxQueueStats.Enabled = true;
			radioButtonRelativePeriodTodayQueueStats.Checked = true;
			radioButtonRelativePeriodQueueStats.Checked = false;
		}

		private void initiateScheduleDatePeriod()
		{
			groupBoxSchedule.Enabled = true;
			numericUpDownRelativePeriodStartSchedule.Value = -60;
			numericUpDownRelativePeriodEndSchedule.Value = 120;
			radioButtonRelativePeriodSchedule.Checked = true;
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			//Close();
		}

		private void comboBoxJob_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboBoxJob.SelectedIndex > -1)
			{
				_selectedJob = (IJob)comboBoxJob.SelectedItem;
				if (comboBoxDataSource.Items.Count == 0 && _selectedJob.NeedsParameterDataSource)
				{
					disableJobScheduleSave();
				}
				else
				{
					comboBoxDataSource.Enabled = _selectedJob.NeedsParameterDataSource;
					updateDescription();
					setDefaultForRelativePeriods(_selectedJob);
				}
			}
			else _selectedJob = null;

		}

		private void disableJobScheduleSave()
		{
			groupBoxQueueStats.Enabled = false;
			groupBoxAgentStats.Enabled = false;
			groupBoxForecast.Enabled = false;
			groupBoxSchedule.Enabled = false;
			groupBoxInitial.Enabled = false;
			buttonOk.Enabled = false;
			comboBoxDataSource.Enabled = false;
			labelDescription.Text =
				 string.Format(CultureInfo.CurrentCulture, "The job '{0}' cannot be scheduled since there are no datasources available.",
									_selectedJob.Name);
		}

		private void buttonOk_Click(object sender, EventArgs e)
		{
			_isOkButtonClicked = true;
			validateScheduleSettings();
			if (_isScheduleSettingsValid)
			{
				prepareSchedule();
				saveJobSchedule();
				//Close();
			}
		}

		private void validateScheduleSettings()
		{
			if (textBoxScheduleName.Text.Length == 0)
			{
				showWarning("Invalid Schedule Name.", "Invalid Setting");
				_isScheduleSettingsValid = false;
				return;
			}

			if (comboBoxJob.SelectedIndex == -1)
			{
				showWarning("No Job selected.", "Invalid Setting");
				_isScheduleSettingsValid = false;
				return;
			}

			if (dateTimePickerStartingAt.Value.TimeOfDay.TotalMinutes > dateTimePickerEndingAt.Value.TimeOfDay.TotalMinutes)
			{
				showWarning("The 'Starting at' time cannot be later than the 'Ending at' time for the Daily Frequency", "Invalid Setting");
				_isScheduleSettingsValid = false;
				return;
			}

			if (!isRelativePeriodsValid())
			{
				showWarning("The relative data period start cannot be higher than the relative data period end.", "Invalid Setting");
				_isScheduleSettingsValid = false;
				return;
			}
			if (comboBoxDataSource.Enabled && comboBoxDataSource.SelectedIndex == -1)
			{
				showWarning("No Log Data Source selected.", "Invalid Setting");
				_isScheduleSettingsValid = false;
				return;
			}

			_isScheduleSettingsValid = true;
		}

		private bool isRelativePeriodsValid()
		{
			if (numericUpDownRelativePeriodStartInitial.Value > numericUpDownRelativePeriodEndInitial.Value
				 || numericUpDownRelativePeriodStartQueueStats.Value > numericUpDownRelativePeriodEndQueueStats.Value
				 || numericUpDownRelativePeriodStartAgentStats.Value > numericUpDownRelativePeriodEndAgentStats.Value
				 || numericUpDownRelativePeriodStartSchedule.Value > numericUpDownRelativePeriodEndSchedule.Value
				 || numericUpDownRelativePeriodStartForecast.Value > numericUpDownRelativePeriodEndForecast.Value)
			{
				return false;
			}
			return true;
		}

		private void showWarning(string message, string caption)
		{
			MessageBox.Show(this, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning,
								 MessageBoxDefaultButton.Button1,
								 (RightToLeft == RightToLeft.Yes)
										? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
										: 0);
		}

		private void prepareSchedule()
		{
			var logDataSource = -1;
			if (comboBoxDataSource.Enabled && comboBoxDataSource.SelectedIndex != -1)
			{
				logDataSource = (int) comboBoxDataSource.SelectedValue;
			}

			int scheduleId;
			string tenantName;
			if (_isNewSchedule)
			{
				scheduleId = -1;
				tenantName = Tenants.NameForOptionAll;
			}
			else
			{
				scheduleId = _etlJobSchedule.ScheduleId;
				tenantName = _etlJobSchedule.TenantName;
			}

			if (radioButtonOccursOnce.Checked)
			{
				_etlJobSchedule = new Common.JobSchedule.EtlJobSchedule(
					scheduleId,
					textBoxScheduleName.Text,
					checkBoxEnabled.Checked,
					(int) dateTimePickerOccursOnce.Value.TimeOfDay.TotalMinutes,
					(string) comboBoxJob.SelectedValue,
					logDataSource,
					labelDescription.Text,
					null,
					getRelativePeriodCollection(),
					tenantName);
			}
			else
			{
				_etlJobSchedule = new Common.JobSchedule.EtlJobSchedule(
					scheduleId,
					textBoxScheduleName.Text,
					checkBoxEnabled.Checked,
					(int) numericUpDownOccursEveryMinute.Value,
					(int) dateTimePickerStartingAt.Value.TimeOfDay.TotalMinutes,
					(int) dateTimePickerEndingAt.Value.TimeOfDay.TotalMinutes,
					(string) comboBoxJob.SelectedValue,
					logDataSource,
					labelDescription.Text,
					null,
					DateTime.Now,
					getRelativePeriodCollection(),
					tenantName);
			}
		}

		private IList<IEtlJobRelativePeriod> getRelativePeriodCollection()
		{
			IList<IEtlJobRelativePeriod> etlJobRelativePeriods = new List<IEtlJobRelativePeriod>();
			IList<JobCategoryType> jobCategoryTypeCollection = new List<JobCategoryType>();

			// Get a distinct list of which job categorys that is included in the selected job
			foreach (IJobStep jobStep in _selectedJob.StepList)
			{
				if (!jobCategoryTypeCollection.Contains(jobStep.JobCategory))
				{
					jobCategoryTypeCollection.Add(jobStep.JobCategory);
				}
			}

			foreach (JobCategoryType jobCategoryType in jobCategoryTypeCollection)
			{
				var minMaxRelativePeriod = new MinMax<int>(0, 0);
				switch (jobCategoryType)
				{
					case JobCategoryType.Initial:
						if (radioButtonRelativePeriodInitial.Checked)
							minMaxRelativePeriod = new MinMax<int>((int)numericUpDownRelativePeriodStartInitial.Value,
																				(int)numericUpDownRelativePeriodEndInitial.Value);
						break;
					case JobCategoryType.QueueStatistics:
						if (radioButtonRelativePeriodQueueStats.Checked)
							minMaxRelativePeriod = new MinMax<int>((int)numericUpDownRelativePeriodStartQueueStats.Value,
																				(int)numericUpDownRelativePeriodEndQueueStats.Value);
						break;
					case JobCategoryType.AgentStatistics:
						if (radioButtonRelativePeriodAgentStats.Checked)
							minMaxRelativePeriod = new MinMax<int>((int)numericUpDownRelativePeriodStartAgentStats.Value,
																				(int)numericUpDownRelativePeriodEndAgentStats.Value);
						break;
					case JobCategoryType.Schedule:
						if (radioButtonRelativePeriodSchedule.Checked)
							minMaxRelativePeriod = new MinMax<int>((int)numericUpDownRelativePeriodStartSchedule.Value,
																				(int)numericUpDownRelativePeriodEndSchedule.Value);
						break;
					case JobCategoryType.Forecast:
						if (radioButtonRelativePeriodForecast.Checked)
							minMaxRelativePeriod = new MinMax<int>((int)numericUpDownRelativePeriodStartForecast.Value,
																				(int)numericUpDownRelativePeriodEndForecast.Value);
						break;
					default:
						minMaxRelativePeriod = new MinMax<int>(int.MinValue, int.MaxValue);
						break;
				}

				if (minMaxRelativePeriod != new MinMax<int>(int.MinValue, int.MaxValue))
				{
					etlJobRelativePeriods.Add(new EtlJobRelativePeriod(minMaxRelativePeriod, jobCategoryType));
				}
			}

			return etlJobRelativePeriods;
		}

		private void saveJobSchedule()
		{
			int scheduleId = _repository.SaveSchedule(_etlJobSchedule);
			_etlJobSchedule.SetScheduleIdOnPersistedItem(scheduleId);
			_repository.SaveSchedulePeriods(_etlJobSchedule);
			if (_isNewSchedule && _observableCollection != null)
			{
				// Add to ObservableCollection
				_observableCollection.Add(_etlJobSchedule);
			}
			else if (_observableCollection != null)
			{
				// Update existing job schedule in ObservableCollection
				int index = _observableCollection.IndexOf(_etlJobScheduleToEdit);
				_observableCollection.RemoveAt(index);
				_observableCollection.Insert(index, _etlJobSchedule);
			}
		}

		private void JobSchedule_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!_isScheduleSettingsValid && _isOkButtonClicked)
			{
				_isOkButtonClicked = false;
				e.Cancel = true;
			}
		}

		private void updateDescription()
		{
			labelDescription.Text = _isNewSchedule ? string.Empty : _etlJobSchedule.Description;
		}
	}
}