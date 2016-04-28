﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using Castle.Core.Internal;
using Stardust.Manager.Diagnostics;
using Stardust.Manager.Extensions;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;
using Timer = System.Timers.Timer;

namespace Stardust.Manager
{
	public class JobManager : IDisposable
	{
		private readonly Timer _checkAndAssignJob = new Timer();
		private readonly Timer _checkHeartbeatsTimer = new Timer();

		private readonly IHttpSender _httpSender;
		private readonly IJobRepository _jobRepository;
		private readonly ManagerConfiguration _managerConfiguration;
		private readonly IWorkerNodeRepository _workerNodeRepository;

		public JobManager(IJobRepository jobRepository,
									IWorkerNodeRepository workerNodeRepository,
									IHttpSender httpSender,
									ManagerConfiguration managerConfiguration)
		{
			if (managerConfiguration.AllowedNodeDownTimeSeconds <= 0)
			{
				this.Log().ErrorWithLineNumber("AllowedNodeDownTimeSeconds must be greater than zero!");

				throw new ArgumentOutOfRangeException("managerConfiguration.AllowedNodeDownTimeSeconds");
			}

			_jobRepository = jobRepository;
			_workerNodeRepository = workerNodeRepository;
			_httpSender = httpSender;
			_managerConfiguration = managerConfiguration;

			_checkAndAssignJob.Elapsed += AssignJobToWorkerNodes_Elapsed;
			_checkAndAssignJob.Interval = _managerConfiguration.CheckNewJobIntervalSeconds * 1000;
			_checkAndAssignJob.Start();

			_checkHeartbeatsTimer.Elapsed += CheckHeartbeats_Elapsed;
			_checkHeartbeatsTimer.Interval = _managerConfiguration.AllowedNodeDownTimeSeconds * 200;
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

			_checkAndAssignJob.Stop();
			_checkAndAssignJob.Dispose();

			_checkHeartbeatsTimer.Stop();
			_checkHeartbeatsTimer.Dispose();

			this.Log().DebugWithLineNumber("Finished disposing.");
		}

		private void AssignJobToWorkerNodes_Elapsed(object sender, ElapsedEventArgs e)
		{
			AssignJobToWorkerNodes();
		}

		public void AssignJobToWorkerNodes()
		{
			_jobRepository.AssignJobToWorkerNode(_httpSender);
		}

		private void CheckHeartbeats_Elapsed(object sender, ElapsedEventArgs e)
		{
			CheckWorkerNodesAreAlive(TimeSpan.FromSeconds(_managerConfiguration.AllowedNodeDownTimeSeconds));
		}

		public void CheckWorkerNodesAreAlive(TimeSpan timeSpan)
		{
			//If two managers find the same node as dead the first one will update the status 
			//and the second one won't find that job as "executing" and do nothing
			var deadNodes = _workerNodeRepository.CheckNodesAreAlive(timeSpan);
			if (deadNodes.IsNullOrEmpty())
				return;

			var jobs = _jobRepository.GetAllExecutingJobs();
			if (jobs.IsNullOrEmpty())
				return;

			foreach (var node in deadNodes)
			{
				foreach (var job in jobs)
				{
					if (job.SentToWorkerNodeUri == node)
					{
						this.Log().ErrorWithLineNumber("Job ( id , name ) is deleted due to the node executing it died. ( " + job.JobId +
						                               " , " + job.Name + " )");

						UpdateResultForJob(job.JobId,
						                   "Fatal Node Failure",
						                   DateTime.UtcNow);
					}
				}
			}
		}

		public IList<WorkerNode> GetAllWorkerNodes()
		{
			return _workerNodeRepository.GetAllWorkerNodes();
		}


		public JobQueueItem GetJobQueueItemByJobId(Guid jobId)
		{
			return _jobRepository.GetJobQueueItemByJobId(jobId);
		}

		public List<JobQueueItem> GetAllItemsInJobQueue()
		{
			return _jobRepository.GetAllItemsInJobQueue();
		}

		public void AddItemToJobQueue(JobQueueItem jobQueueItem)
		{
			_jobRepository.AddItemToJobQueue(jobQueueItem);
		}

		public bool DoesJobDetailItemExists(Guid jobId)
		{
			return _jobRepository.DoesJobDetailItemExists(jobId);
		}

		public bool DoesJobItemItemExists(Guid jobId)
		{
			return _jobRepository.DoesJobItemExists(jobId);
		}

		public void DeleteJobQueueItemByJobId(Guid jobId)
		{
			_jobRepository.DeleteJobQueueItemByJobId(jobId);
		}

		public void DeleteJobByJobId(Guid jobId,
									 bool removeJobDetails)
		{
			_jobRepository.DeleteJobByJobId(jobId,
											removeJobDetails);
		}

		public bool DoesJobQueueItemExists(Guid jobId)
		{
			return _jobRepository.DoesJobQueueItemExists(jobId);
		}

		public void CancelJobByJobId(Guid jobId)
		{
			_jobRepository.CancelJobByJobId(jobId,
											_httpSender);
		}

		public void UpdateResultForJob(Guid jobId,
									   string result,
									   DateTime ended)
		{
			_jobRepository.UpdateResultForJob(jobId,
											  result,
											  ended);
		}

		public void CreateJobDetail(JobDetail jobDetail)
		{
			_jobRepository.CreateJobDetailByJobId(jobDetail.JobId,
												  jobDetail.Detail,
												  jobDetail.Created);
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