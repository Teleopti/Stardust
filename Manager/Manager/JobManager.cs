using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using log4net;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;
using Stardust.Manager.Timers;

namespace Stardust.Manager
{ 
	public class JobManager : IDisposable, IJobManager
    {
		private readonly Timer _checkAndAssignJob = new Timer();
		private readonly Timer _checkHeartbeatsTimer = new Timer();

		private readonly IJobRepository _jobRepository;
		private readonly ManagerConfiguration _managerConfiguration;
		private readonly IWorkerNodeRepository _workerNodeRepository;
		private readonly NodeManager _nodeManager;

		private readonly ILog ManagerLogger;
		//private static readonly ILog ManagerLogger = LogManager.GetLogger("Stardust.ManagerLog");

		public JobManager(IJobRepository jobRepository,
		                  IWorkerNodeRepository workerNodeRepository,
		                  ManagerConfiguration managerConfiguration,
						  JobPurgeTimer jobPurgeTimer,
						  NodePurgeTimer nodePurgeTimer, 
						  NodeManager nodeManager,
						  ILog logger)
		{
			_jobRepository = jobRepository;
			_workerNodeRepository = workerNodeRepository;
			_managerConfiguration = managerConfiguration;
			_nodeManager = nodeManager;
			ManagerLogger = logger;

			_checkAndAssignJob.Elapsed += AssignJobToWorkerNodes_Elapsed;
			_checkAndAssignJob.Interval = _managerConfiguration.CheckNewJobIntervalSeconds*1000;
			_checkAndAssignJob.Start();

			_checkHeartbeatsTimer.Elapsed += CheckHeartbeats_Elapsed;
			_checkHeartbeatsTimer.Interval = _managerConfiguration.AllowedNodeDownTimeSeconds*1000;
			_checkHeartbeatsTimer.Start();

			jobPurgeTimer.Start();
			nodePurgeTimer.Start();
		}

		public void Dispose()
		{
			_checkAndAssignJob.Stop();
			_checkAndAssignJob.Dispose();

			_checkHeartbeatsTimer.Stop();
			_checkHeartbeatsTimer.Dispose();
		}

		private void AssignJobToWorkerNodes_Elapsed(object sender, ElapsedEventArgs e)
		{
			AssignJobToWorkerNodes();
		}

		public void AssignJobToWorkerNodes()
		{
			ManagerLogger.Info("Going to assign job to the nodes in timer");
			_checkAndAssignJob.Enabled = false;
			_jobRepository.AssignJobToWorkerNode();
			_checkAndAssignJob.Enabled = true;
		}

		private void CheckHeartbeats_Elapsed(object sender, ElapsedEventArgs e)
		{
			CheckWorkerNodesAreAlive(TimeSpan.FromSeconds(_managerConfiguration.AllowedNodeDownTimeSeconds));
		}

		public void CheckWorkerNodesAreAlive(TimeSpan timeSpan)
		{
			var deadNodes = _workerNodeRepository.CheckNodesAreAlive(timeSpan);
			if (!deadNodes.Any())
			{
				return;
			}

			foreach (var node in deadNodes)
			{
				_nodeManager.RequeueJobsThatDidNotFinishedByWorkerNodeUri(node);
			}
		}
        
		public void AddItemToJobQueue(JobQueueItem jobQueueItem)
		{
			_jobRepository.AddItemToJobQueue(jobQueueItem);
		}
		
		public void CancelJobByJobId(Guid jobId)
		{
			_jobRepository.CancelJobByJobId(jobId);
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