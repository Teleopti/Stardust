﻿using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;
using System.Timers;

namespace Stardust.Manager
{
	public class JobManager
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (JobManager));
		private readonly IHttpSender _httpSender;
		private readonly IJobRepository _jobRepository;
		private readonly IWorkerNodeRepository _workerNodeRepository;
		private readonly IManagerConfiguration _managerConfiguration;
		private readonly Timer _checkHeartbeatsTimer = new Timer();

		public JobManager(IJobRepository jobRepository,
		                  IWorkerNodeRepository workerNodeRepository,
		                  IHttpSender httpSender,
						  IManagerConfiguration managerConfiguration)
		{
			if (managerConfiguration.AllowedNodeDownTimeSeconds <= 0)
			{
				LogHelper.LogErrorWithLineNumber(Logger, "AllowedNodeDownTimeSeconds is not greater than zero!");
				throw new ArgumentOutOfRangeException();
			}
		
			_jobRepository = jobRepository;
			_workerNodeRepository = workerNodeRepository;
			_httpSender = httpSender;
			_managerConfiguration = managerConfiguration;

			_checkHeartbeatsTimer.Elapsed += OnTimedEvent;
			_checkHeartbeatsTimer.Interval = _managerConfiguration.AllowedNodeDownTimeSeconds * 500; //NodeDowntime in milliseconds divided by 2
			_checkHeartbeatsTimer.Start();
			
		}

		public void CheckNodesAreAlive(TimeSpan timeSpan)
		{
			List<string> deadNodes = _workerNodeRepository.CheckNodesAreAlive(timeSpan);

			List<JobDefinition> jobs = _jobRepository.LoadAll();

			if (deadNodes != null)
			{
				foreach (string node in deadNodes)
				{
					foreach (JobDefinition job in jobs)
					{
						if (job.AssignedNode == node)
						{
							LogHelper.LogErrorWithLineNumber(Logger, "Job ( id , name ) is deleted due to the node executing it died. ( " + job.Id + " , " + job.Name + " )");
							SetEndResultOnJobAndRemoveIt(job.Id, "fatal");
						}
					}
				}
			}
		}

		private void OnTimedEvent(object sender,
								ElapsedEventArgs e)
		{
			CheckNodesAreAlive(TimeSpan.FromSeconds(_managerConfiguration.AllowedNodeDownTimeSeconds));
			LogHelper.LogDebugWithLineNumber(Logger, " Check Heartbeat");
		}

		public IList<WorkerNode> Nodes()
		{
			return _workerNodeRepository.LoadAll();
		}


		public IList<WorkerNode> UpNodes()
		{
			var upNodes = new List<WorkerNode>();
			var availableNodes = _workerNodeRepository.LoadAllFreeNodes();
			foreach (var availableNode in availableNodes)
			{
				var nodeUriBuilder = new NodeUriBuilderHelper(availableNode.Url);
				var postUri = nodeUriBuilder.GetIsAliveTemplateUri();
				var success = _httpSender.TryGetAsync(postUri);
				if (success == null || success.Result)
				{
					upNodes.Add(availableNode);
				}
			}
			return upNodes;
		}

		public void RegisterHeartbeat(string nodeUri)
		{
			LogHelper.LogDebugWithLineNumber(Logger,
											 "Start RegisterHeartbeat.");

			_workerNodeRepository.RegisterHeartbeat(nodeUri);

			LogHelper.LogDebugWithLineNumber(Logger,
												 "Finished RegisterHeartbeat.");
		}

		public void CheckAndAssignNextJob()
		{
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Start CheckAndAssignNextJob.");

			try
			{
				var availableNodes = _workerNodeRepository.LoadAllFreeNodes();

				var upNodes = new List<WorkerNode>();

				if (availableNodes != null && availableNodes.Any())
				{
					LogHelper.LogDebugWithLineNumber(Logger,
					                                 "Found ( " + availableNodes.Count + " ) available nodes");
				}

				foreach (var availableNode in availableNodes)
				{
					var nodeUriBuilder = new NodeUriBuilderHelper(availableNode.Url);

					var postUri = nodeUriBuilder.GetIsAliveTemplateUri();

					LogHelper.LogDebugWithLineNumber(Logger,
					                                 "Test available node is alive : Url ( " + postUri + " )");


					var success = _httpSender.TryGetAsync(postUri);

					if (success.Result)
					{
						LogHelper.LogDebugWithLineNumber(Logger,
						                                 "Node Url ( " + postUri + " ) is available and alive.");

						upNodes.Add(availableNode);
					}
				}

				_jobRepository.CheckAndAssignNextJob(upNodes,
				                                     _httpSender);

				LogHelper.LogDebugWithLineNumber(Logger,
				                                 "Finished CheckAndAssignNextJob.");
			}

			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger, exp.Message, exp);
				throw;
			}
		}

		public void Add(JobDefinition job)
		{
			_jobRepository.Add(job);
		}

		public void CancelThisJob(Guid id)
		{
			_jobRepository.CancelThisJob(id,
			                             _httpSender);
		}

		public void SetEndResultOnJobAndRemoveIt(Guid jobId,
		                                         string result)
		{
			_jobRepository.SetEndResultOnJob(jobId,
			                                 result);

			_jobRepository.DeleteJob(jobId);
		}

		public void ReportProgress(JobProgressModel model)
		{
			_jobRepository.ReportProgress(model.JobId,
			                              model.ProgressDetail);
		}

		public JobHistory GetJobHistory(Guid jobId)
		{
			return _jobRepository.History(jobId);
		}

		public IList<JobHistory> GetJobHistoryList()
		{
			return _jobRepository.HistoryList();
		}

		public IList<JobHistoryDetail> JobHistoryDetails(Guid jobId)
		{
			return _jobRepository.JobHistoryDetails(jobId);
		}

	}
}