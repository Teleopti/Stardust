using System;
using System.Windows.Forms;
using Autofac;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.ConfigTool.Transformer;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.Control
{
	/// <summary>
	/// Interaction logic for ManualEtl.xaml
	/// </summary>
	public partial class ManualEtl : IDisposable
	{
		private readonly BackGroundJob _backGroundJob = new BackGroundJob();
		private IBaseConfiguration _baseConfiguration;
		
		public event EventHandler<AlarmEventArgs> InitialJobNowAvailable;
		public event EventHandler<AlarmEventArgs> JobStartedRunning;
		public event EventHandler<AlarmEventArgs> JobStoppedRunning;

		public ManualEtl()
		{
			InitializeComponent();

			myTree.JobRun += etlControlJobRun;
			myTree.JobSelectionChanged += myTree_JobSelectionChanged;
			myTree.InitialJobNowAvailable += myTree_InitialJobNowAvailable;
			_backGroundJob.JobStartedRunning += _backGroundJob_JobStartedRunning;
			_backGroundJob.JobStoppedRunning += _backGroundJob_JobStoppedRunning;
		}

		private void _backGroundJob_JobStoppedRunning(object sender, AlarmEventArgs e)
		{
			JobStoppedRunning(sender, new AlarmEventArgs(e.Job));
			myTree.SetExecuteEnabledState(true);
		}

		private void _backGroundJob_JobStartedRunning(object sender, AlarmEventArgs e)
		{
			JobStartedRunning(sender, new AlarmEventArgs(e.Job));
		}

		private void myTree_InitialJobNowAvailable(object sender, AlarmEventArgs e)
		{
			myControl.SetTenantDataSource(myTree.TenantCollection);
			InitialJobNowAvailable(sender, new AlarmEventArgs(e.Job));
		}

		private void myTree_JobSelectionChanged(object sender, AlarmEventArgs e)
		{
			myControl.UpdateControls(e.Job);
		}

		private void etlControlJobRun(object sender, AlarmEventArgs e)
		{
			e.Job.StepList[0].JobParameters.DataSource = myControl.LogDataSource;
			_baseConfiguration.JobHelper.SelectDataSourceContainer(myControl.TenantName);
			//Clear job periods before adding them again
			e.Job.StepList[0].JobParameters.JobCategoryDates.Clear();

			foreach (JobCategoryType jobCategoryType in myControl.JobMultipleDatePeriods.JobCategoryCollection)
			{
				e.Job.StepList[0].JobParameters.JobCategoryDates.Add(
					 myControl.JobMultipleDatePeriods.GetJobMultipleDateItem(jobCategoryType), jobCategoryType);
			}

			if (isDatesValid(e.Job.StepList[0].JobParameters.JobCategoryDates))
				RunJob(e.Job);
		}

		private bool isDatesValid(IJobMultipleDate jobMultipleDate)
		{
			foreach (IJobMultipleDateItem jobMultipleDateItem in jobMultipleDate.AllDatePeriodCollection)
			{
				if (jobMultipleDateItem.StartDateLocal >= jobMultipleDateItem.EndDateLocal)
				{
					const string message = "'Date from' cannot be set to a later date than 'Date to'. Change date selection and try again.";
					const string caption = "Invalid date selection";
					MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, 0);
					myTree.SetExecuteEnabledState(true);
					return false;
				}
			}
			return true;
		}

		internal void RunJob(IJob job)
		{
			_backGroundJob.Run(job);
		}

		internal void ReloadDataSources()
		{
			myControl.ReloadDataSourceComboBox();
			myTree.SetEnableStateForJobCollection();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			_backGroundJob.Dispose();
			Dispose();
		}

		public void SetBaseConfiguration(IBaseConfiguration baseConfiguration, IContainer container)
		{
			_baseConfiguration = baseConfiguration;
			myControl.SetBaseConfiguration(baseConfiguration);
			myTree.LoadJobTree(baseConfiguration, container);
		}

		public ManualControl ManualControl
		{ get { return myControl; } }

		
	}
}
