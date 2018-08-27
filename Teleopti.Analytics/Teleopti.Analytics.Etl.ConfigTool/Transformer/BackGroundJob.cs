using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using log4net;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Ccc.Infrastructure.DistributedLock;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.ConfigTool.Transformer
{
	class BackGroundJob : IDisposable
	{
		private readonly BackgroundWorker _backgroundWorker = new BackgroundWorker();
		private readonly ILog _logger = LogManager.GetLogger(typeof(BackGroundJob));
		public event EventHandler<AlarmEventArgs> JobStartedRunning;
		public event EventHandler<AlarmEventArgs> JobStoppedRunning;

		public BackGroundJob()
		{
			_backgroundWorker.DoWork += bw_DoWork;
			_backgroundWorker.RunWorkerCompleted += bw_RunWorkerCompleted;
		}

		private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Cancelled)
			{
				Trace.WriteLine("Background Worker cancelled.");
			}
			else if (e.Error != null)
			{
				_logger.Error("Background thread exception", e.Error);
				MessageBox.Show(e.Error.ToString(), "Background thread exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else
			{
				// Job execution ready
				if (e.Result is JobExecutionState jobExecutionState)
				{
					jobExecutionState.Job.NotifyJobExecutionReady();
					JobStoppedRunning?.Invoke(sender, new AlarmEventArgs(jobExecutionState.Job));
					NotifyUser(jobExecutionState);
				}
			}
		}

		private static void NotifyUser(JobExecutionState jobExecutionState)
		{
			if (!jobExecutionState.IsEtlJobLocked) return;

			var msg = new StringBuilder("Another job is running at the moment. Please try again later.\n\n");
			msg.Append(string.Format(CultureInfo.InvariantCulture, "Server name: {0}\n", jobExecutionState.EtlRunningInformation.ComputerName));
			msg.Append(string.Format(CultureInfo.InvariantCulture, "Job Name: {0}\n", jobExecutionState.EtlRunningInformation.JobName));
			msg.Append(string.Format(CultureInfo.InvariantCulture, "Start Time: {0}\n", jobExecutionState.EtlRunningInformation.StartTime));
			msg.Append(string.Format(CultureInfo.InvariantCulture, "Run By ETL Service: {0}\n",
				jobExecutionState.EtlRunningInformation.IsStartedByService ? "Yes" : "No"));
			MessageBox.Show(msg.ToString(), @"Information", MessageBoxButtons.OK, MessageBoxIcon.Information,
				MessageBoxDefaultButton.Button1, 0);
		}

		private void bw_DoWork(object sender, DoWorkEventArgs e)
		{
			var jobResultCollection = new List<IJobResult>();
			var jobStepsNotToRun = new List<IJobStep>();
			var argument = (RunJobArgument)e.Argument;
			var job = (JobBase)argument.Job;
			setCultureOnThread(job);
			var connectionString = ConfigurationManager.AppSettings["datamartConnectionString"];
			var repository = new Repository(connectionString);
			var runController = new RunController(repository);

			if (runController.CanIRunAJob(out var etlRunningInformation))
			{
				try
				{
					using (var etlJobLock = new EtlJobLock(connectionString, job.Name, false))
					{
						var jobRunner = new JobRunner();
						var jobResults = jobRunner.Run(job, jobResultCollection, jobStepsNotToRun);
						if (jobResults != null && jobResults.Any())
						{
							var exception = jobResults.First().JobStepResultCollection.First().JobStepException;
							if(exception != null && exception.Message.Contains("license"))
								MessageBox.Show(@"Please apply a license from the main client before ETL job is run.", @"Warning",
									MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, 0);

							jobRunner.SaveResult(jobResults, repository, -1, argument.TenantName);
						}
					}
				}
				catch (DistributedLockException)
				{
					if (runController.CanIRunAJob(out etlRunningInformation))
					{
						// Actually there was not any job running.
						throw;
					}
				}
			}

			e.Result = new JobExecutionState(etlRunningInformation, job);
		}

		private static void setCultureOnThread(JobBase job)
		{
			Thread.CurrentThread.CurrentCulture = job.JobParameters.CurrentCulture;
		}

		public void Run(IJob job, string tenantName)
		{
			if (_backgroundWorker.IsBusy) return;

			JobStartedRunning?.Invoke(this, new AlarmEventArgs(job));
			_backgroundWorker.RunWorkerAsync(new RunJobArgument{Job = job, TenantName = tenantName});
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

	internal class RunJobArgument
	{
		public IJob Job { get; set; }
		public string TenantName { get; set; }
	}
}