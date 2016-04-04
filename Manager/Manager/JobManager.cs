using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using Stardust.Manager.Diagnostics;
using Stardust.Manager.Extensions;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;
using Timer = System.Timers.Timer;

namespace Stardust.Manager
{
	public class JobManager : IDisposable
	{
		private readonly Timer _checkAndAssignNextJob = new Timer();
		private readonly Timer _checkHeartbeatsTimer = new Timer();

		private readonly IHttpSender _httpSender;
		private readonly IJobRepository _jobRepository;
		private readonly IManagerConfiguration _managerConfiguration;
		private readonly IWorkerNodeRepository _workerNodeRepository;

		public JobManager(IJobRepository jobRepository,
		                  IWorkerNodeRepository workerNodeRepository,
		                  IHttpSender httpSender,
		                  IManagerConfiguration managerConfiguration)
		{
			if (managerConfiguration.AllowedNodeDownTimeSeconds <= 0)
			{
				this.Log().ErrorWithLineNumber("AllowedNodeDownTimeSeconds is not greater than zero!");

				throw new ArgumentOutOfRangeException();
			}

			_jobRepository = jobRepository;
			_workerNodeRepository = workerNodeRepository;
			_httpSender = httpSender;
			_managerConfiguration = managerConfiguration;

			_checkAndAssignNextJob.Elapsed += _checkAndAssignNextJob_Elapsed;
			_checkAndAssignNextJob.Interval = _managerConfiguration.CheckNewJobIntervalSeconds*1000;
			_checkAndAssignNextJob.Start();

			_checkHeartbeatsTimer.Elapsed += CheckHeartbeatsOnTimedEvent;
			_checkHeartbeatsTimer.Interval = _managerConfiguration.AllowedNodeDownTimeSeconds*200;
			_checkHeartbeatsTimer.Start();

			var workerNodes = _workerNodeRepository.LoadAll();
			if (workerNodes.Any())
			{
				foreach (var workerNode in workerNodes)
				{
					_workerNodeRepository.RegisterHeartbeat(workerNode.Url.ToString(), false);
				}
			}
		}

		public void Dispose()
		{
			this.Log().DebugWithLineNumber("Start disposing.");

			_checkAndAssignNextJob.Stop();
			_checkAndAssignNextJob.Dispose();

			_checkHeartbeatsTimer.Stop();
			_checkHeartbeatsTimer.Dispose();

			this.Log().DebugWithLineNumber("Finished disposing.");
		}

		private void _checkAndAssignNextJob_Elapsed(object sender, ElapsedEventArgs e)
		{
			this.Log().DebugWithLineNumber("Start.");

			CheckAndAssignNextJob();

			this.Log().DebugWithLineNumber("CheckAndAssignNextJob on thread id : " + Thread.CurrentThread.ManagedThreadId);
		}


		public void CheckNodesAreAlive(TimeSpan timeSpan)
		{
			var deadNodes = _workerNodeRepository.CheckNodesAreAlive(timeSpan);

			var jobs = _jobRepository.GetAllJobDefinitions();

			if (deadNodes != null && jobs != null)
			{
				foreach (var node in deadNodes)
				{
					foreach (var job in jobs)
					{
						if (job.AssignedNode == node)
						{
							this.Log().ErrorWithLineNumber("Job ( id , name ) is deleted due to the node executing it died. ( " + job.Id +
							                              " , " + job.Name + " )");

							SetEndResultOnJobAndRemoveIt(job.Id, "Fatal Node Failure");
						}
					}
				}
			}
		}

		private void CheckHeartbeatsOnTimedEvent(object sender,
		                                         ElapsedEventArgs e)
		{
			CheckNodesAreAlive(TimeSpan.FromSeconds(_managerConfiguration.AllowedNodeDownTimeSeconds));

			this.Log().DebugWithLineNumber("Check Heartbeat on thread id : " + Thread.CurrentThread.ManagedThreadId);
		}

		public IList<WorkerNode> Nodes()
		{
			return _workerNodeRepository.LoadAll();
		}

		public void RegisterHeartbeat(string nodeUri)
		{
			this.Log().DebugWithLineNumber("Start RegisterHeartbeat.");

			_workerNodeRepository.RegisterHeartbeat(nodeUri, true);

			this.Log().DebugWithLineNumber("Finished RegisterHeartbeat.");
		}


		public void CheckAndAssignNextJob()
		{
			this.Log().DebugWithLineNumber("Start CheckAndAssignNextJob.");

			var managerStopWatch = new ManagerStopWatch();

			try
			{
					_jobRepository.CheckAndAssignNextJob(_httpSender);

					this.Log().DebugWithLineNumber("Finished CheckAndAssignNextJob.");
			}

			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message,
				                              exp);

				throw;
			}

			finally
			{
				var total =
					managerStopWatch.GetTotalElapsedTimeInMilliseconds();
				this.Log().DebugWithLineNumber("CheckAndAssignNextJob: Stop ManagerStopWatch. Took " + total + " milliseconds.");
			}
		}

		public void Add(JobDefinition job)
		{
			_jobRepository.AddJobDefinition(job);
		}

		public void CancelThisJob(Guid id)
		{
			_jobRepository.CancelJobByJobId(id,
			                             _httpSender);
		}

		public void SetEndResultOnJobAndRemoveIt(Guid jobId,
		                                         string result)
		{
			_jobRepository.SetEndResultOnJob(jobId,
			                                 result);

			_jobRepository.DeleteJobByJobId(jobId);
		}

		public void ReportProgress(JobProgressModel model)
		{
			_jobRepository.ReportProgress(model.JobId,
			                              model.ProgressDetail,
			                              model.Created);
		}

		public JobHistory GetJobHistory(Guid jobId)
		{
			return _jobRepository.GetJobHistoryByJobId(jobId);
		}

		public IList<JobHistory> GetJobHistoryList()
		{
			return _jobRepository.GetAllJobHistories();
		}

		public IList<JobHistoryDetail> JobHistoryDetails(Guid jobId)
		{
			return _jobRepository.GetJobHistoryDetailsByJobId(jobId);
		}
	}
}