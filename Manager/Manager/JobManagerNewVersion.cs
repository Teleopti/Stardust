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
	public class JobManagerNewVersion : IDisposable
	{
		private readonly Timer _checkAndAssignNextJob = new Timer();
		private readonly Timer _checkHeartbeatsTimer = new Timer();

		private readonly IHttpSender _httpSender;
		private readonly IJobRepository _jobRepository;
		private readonly ManagerConfiguration _managerConfiguration;
		private readonly IWorkerNodeRepository _workerNodeRepository;

		public JobManagerNewVersion(IJobRepository jobRepository,
		                            IWorkerNodeRepository workerNodeRepository,
		                            IHttpSender httpSender,
		                            ManagerConfiguration managerConfiguration)
		{
			if (managerConfiguration.AllowedNodeDownTimeSeconds <= 0)
			{
				this.Log().ErrorWithLineNumber("AllowedNodeDownTimeSeconds must be greater than zero!");

				throw new ArgumentOutOfRangeException("AllowedNodeDownTimeSeconds");
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

			var workerNodes = _workerNodeRepository.GetAllWorkerNodes();

			if (workerNodes != null && workerNodes.Any())
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

			TryAssignJobToWorkerNode();

			this.Log().DebugWithLineNumber("CheckAndAssignNextJob on thread id : " + Thread.CurrentThread.ManagedThreadId);
		}


		public void CheckWorkerNodesAreAlive(TimeSpan timeSpan)
		{
			var deadNodes = _workerNodeRepository.CheckNodesAreAlive(timeSpan);

			var jobs = _jobRepository.GetAllExecutingJobs();

			if (deadNodes != null && deadNodes.Any()
			    && jobs != null && jobs.Any())
			{
				foreach (var node in deadNodes)
				{
					foreach (var job in jobs)
					{
						if (job.SentToWorkerNodeUri == node)
						{
							this.Log().ErrorWithLineNumber("Job ( id , name ) is deleted due to the node executing it died. ( " + job.JobId +
							                               " , " + job.Name + " )");

							SetEndResultOnJob(job.JobId, 
											  "Fatal Node Failure", 
											  DateTime.UtcNow);
						}
					}
				}
			}
		}

		private void CheckHeartbeatsOnTimedEvent(object sender,
		                                         ElapsedEventArgs e)
		{
			CheckWorkerNodesAreAlive(TimeSpan.FromSeconds(_managerConfiguration.AllowedNodeDownTimeSeconds));

			this.Log().DebugWithLineNumber("Check Heartbeat on thread id : " + Thread.CurrentThread.ManagedThreadId);
		}

		public IList<WorkerNode> GetAllWorkerNodes()
		{
			return _workerNodeRepository.GetAllWorkerNodes();
		}

		public void RegisterHeartbeat(string nodeUri)
		{
			this.Log().DebugWithLineNumber("Start RegisterHeartbeat.");

			_workerNodeRepository.RegisterHeartbeat(nodeUri, true);

			this.Log().DebugWithLineNumber("Finished RegisterHeartbeat.");
		}

		public void TryAssignJobToWorkerNode()
		{
			this.Log().DebugWithLineNumber("Start TryAssignJobToWorkerNode.");

			var managerStopWatch = new ManagerStopWatch();

			try
			{
				_jobRepository.TryAssignJobToWorkerNode(_httpSender);

				this.Log().DebugWithLineNumber("Finished TryAssignJobToWorkerNode.");
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

				this.Log().DebugWithLineNumber("TryAssignJobToWorkerNode: Stop ManagerStopWatch. Took " + total + " milliseconds.");
			}
		}

		public void AddItemToJobQueue(JobQueueItem job)
		{
			_jobRepository.AddItemToJobQueue(job);
		}

		public void CancelJobByJobId(Guid jobId)
		{
			_jobRepository.CancelJobByJobId(jobId,
			                                _httpSender);
		}

		public void SetEndResultOnJob(Guid jobId,
		                              string result,
									  DateTime ended)
		{
			_jobRepository.SetEndResultForJob(jobId,
			                                 result,
											 ended);
		}

		public void ReportProgress(JobProgressModel model)
		{
			_jobRepository.ReportProgress(model.JobId,
			                              model.Detail,
			                              model.Created);
		}

		public Job GetJobByJobId(Guid jobId)
		{
			return _jobRepository.GetJobByJobId(jobId);
		}

		public IList<Job> GetAllJobs()
		{
			return _jobRepository.GetAllJobs();
		}

		public IList<JobDetail> GetJobDetailsByJobId(Guid jobId)
		{
			return _jobRepository.GetJobDetailsByJobId(jobId);
		}
	}
}