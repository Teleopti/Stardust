using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using Teleopti.Analytics.Etl.Common.Database;
using Teleopti.Analytics.Etl.Common.Database.EtlLogs;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
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
                var job = e.Result as IJob;
                if (job != null)
                {
                    job.NotifyJobExecutionReady();
                    JobStoppedRunning(sender, new AlarmEventArgs(job));
                    
                }
            }
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
			IList<IJobResult> jobResultCollection = new List<IJobResult>();
			IList<IJobStep> jobStepsNotToRun = new List<IJobStep>();
			var job = (JobBase)e.Argument;
        	SetCultureOnThread(job);
            
            bool firstBuRun = true;
            IList<IBusinessUnit> businessUnitCollection = job.JobParameters.Helper.BusinessUnitCollection;
            // Iterate through bu list and run job for each bu.
            if (businessUnitCollection != null && businessUnitCollection.Count > 0)
            {
                IBusinessUnit lastBuToRun = businessUnitCollection[businessUnitCollection.Count - 1];
                foreach (IBusinessUnit businessUnit in businessUnitCollection)
                {
                    DateTime startTime = DateTime.Now;
                    IJobResult jobResult = job.Run(businessUnit, jobStepsNotToRun, jobResultCollection, firstBuRun, businessUnit.Id == lastBuToRun.Id);
                    jobResult.EndTime = DateTime.Now;
                    jobResult.StartTime = startTime;

                    jobResultCollection.Add(jobResult);
                    firstBuRun = false;
                }
            }

			var rep = new Repository(ConfigurationManager.AppSettings["datamartConnectionString"]);

            if (jobResultCollection.Count > 0)
            {
                foreach (IJobResult jobResult in jobResultCollection)
                {
                    IEtlLog etlLogItem = new EtlLog(rep);
                    etlLogItem.Init(-1, jobResult.StartTime, jobResult.EndTime);

                    foreach (IJobStepResult jobStepResult in jobResult.JobStepResultCollection)
                    {
                        etlLogItem.PersistJobStep(jobStepResult);
                    }

                    etlLogItem.Persist(jobResult);
                }
            }

            e.Result = job;
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
}