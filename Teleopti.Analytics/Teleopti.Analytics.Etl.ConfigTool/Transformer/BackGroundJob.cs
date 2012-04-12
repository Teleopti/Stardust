using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Database;
using Teleopti.Analytics.Etl.Common.Database.EtlLogs;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.ConfigTool.Transformer
{
    class BackGroundJob : IDisposable
    {
        private readonly BackgroundWorker _backgroundWorker = new BackgroundWorker();
        public event EventHandler<AlarmEventArgs> JobStartedRunning;
        public event EventHandler<AlarmEventArgs> JobStoppedRunning;

        public BackGroundJob()
        {
            _backgroundWorker.DoWork += bw_DoWork;
            _backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Trace.WriteLine("Background Worker cancelled.");
            }
            else if (e.Error != null)
            {
                Trace.WriteLine("Error in Background Worker.");
            }
            else
            {
                // Job execution ready
				var jobExecutionState = e.Result as JobExecutionState;
                if (jobExecutionState != null)
                {
                    jobExecutionState.Job.NotifyJobExecutionReady();
                    JobStoppedRunning(sender, new AlarmEventArgs(jobExecutionState.Job));
					notifyUser(jobExecutionState);
                }
            }
        }

		private void notifyUser(JobExecutionState jobExecutionState)
    	{
			if (jobExecutionState.IsEtlJobLocked)
			{
				var msg = new StringBuilder("Another job is running at the moment. Please try again later.\n\n");
				msg.Append(string.Format("Server name: {0}\n", jobExecutionState.EtlRunningInformation.ComputerName));
				msg.Append(string.Format("Job Name: {0}\n", jobExecutionState.EtlRunningInformation.JobName));
				msg.Append(string.Format("Start Time: {0}\n", jobExecutionState.EtlRunningInformation.StartTime));
				msg.Append(string.Format("Run By ETL Service: {0}\n",
				                         jobExecutionState.EtlRunningInformation.IsStartedByService ? "Yes" : "No"));
				MessageBox.Show(msg.ToString(), "Information", MessageBoxButtons.OK);
			}
    	}

    	private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
			IList<IJobResult> jobResultCollection = new List<IJobResult>();
			IList<IJobStep> jobStepsNotToRun = new List<IJobStep>();
			var job = (JobBase)e.Argument;
        	SetCultureOnThread(job);
        	string connectionString = ConfigurationManager.AppSettings["datamartConnectionString"];
        	var repository = new Repository(connectionString);
        	var runController = new RunController(repository);
        	IEtlRunningInformation etlRunningInformation;
			if (runController.CanIRunAJob(out etlRunningInformation))
			{
				runController.StartEtlJobRunLock(job.Name, false, new EtlJobLock(connectionString));
				var jobRunner=new JobRunner();
				IList<IBusinessUnit> businessUnits = job.JobParameters.Helper.BusinessUnitCollection;
				IList<IJobResult> jobResults = jobRunner.Run(job, businessUnits, jobResultCollection, jobStepsNotToRun);
				jobRunner.SaveResult(jobResults, repository, -1);
				runController.Dispose();
			}
			e.Result = new JobExecutionState(etlRunningInformation,job);
        }

    	private static void SetCultureOnThread(JobBase job)
    	{
    		Thread.CurrentThread.CurrentCulture = job.JobParameters.CurrentCulture;
    	}

    	public void Run(IJob job)
        {
            if (!_backgroundWorker.IsBusy)
            {
                JobStartedRunning(this, new AlarmEventArgs(job));
                _backgroundWorker.RunWorkerAsync(job);
            }
        }

        public void Dispose()
        {
            _backgroundWorker.Dispose();
            GC.SuppressFinalize(this);
        }
    }
	internal class JobExecutionState
	{
		public JobExecutionState(IEtlRunningInformation etlRunningInformation, IJob job)
		{
			EtlRunningInformation = etlRunningInformation;
			Job = job;
		}

		public bool IsEtlJobLocked
		{
			get { return EtlRunningInformation != null; }
		}
		public IEtlRunningInformation EtlRunningInformation { get; set; }
		public IJob Job { get; set; }
	}
}